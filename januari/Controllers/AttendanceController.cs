using System.Text.Json;
using ClosedXML.Excel;
using LibraryAttendance.Models;
using LibraryAttendance.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using X.PagedList;
using X.PagedList.Extensions;

namespace LibraryAttendance.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 Method untuk membaca data mahasiswa dari file JSON
        private Dictionary<string, string> LoadStudentData()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "students.json");

            if (!System.IO.File.Exists(filePath))
            {
                return new Dictionary<string, string>();
            }

            var jsonData = System.IO.File.ReadAllText(filePath);
            var students = JsonSerializer.Deserialize<List<Student>>(jsonData);

            return students?.ToDictionary(s => s.NIM, s => s.Name) ?? new Dictionary<string, string>();
        }

        // 🔹 Halaman Absensi (Index)
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string nim, string mode)
        {
            if (!string.IsNullOrEmpty(nim) && !string.IsNullOrEmpty(mode))
            {
                // Pastikan hanya menyimpan 9 digit terakhir dari NIM
                nim = nim.Length > 9 ? nim.Substring(nim.Length - 9) : nim;

                var attendance = new Attendance
                {
                    NIM = nim,
                    Mode = mode,
                    Timestamp = DateTime.Now
                };

                _context.Attendances.Add(attendance);
                _context.SaveChanges();
                TempData["Message"] = "Absensi berhasil!";
            }
            return RedirectToAction("Index");
        }

        // 🔹 Halaman Laporan Kehadiran
        public IActionResult Laporan(DateTime? startDate, DateTime? endDate)
        {
            var studentData = LoadStudentData();

            var data = _context.Attendances.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {
                data = data.Where(a => a.Timestamp >= startDate.Value.Date && a.Timestamp < endDate.Value.Date.AddDays(1));
            }
            else if (startDate.HasValue)
            {
                data = data.Where(a => a.Timestamp.Date == startDate.Value.Date);
            }

            var attendanceList = data.ToList();
            var attendanceCount = attendanceList
                .Where(a => a.Mode == "Keluar")
                .GroupBy(a => a.NIM)
                .ToDictionary(g => g.Key, g => g.Count());

            var attendanceGrouped = attendanceList
                .GroupBy(a => new { a.NIM, a.Timestamp.Date })
                .Select(g =>
                {
                    var Masuk = g.Where(a => a.Mode == "Masuk").OrderBy(a => a.Timestamp).FirstOrDefault();
                    var Keluar = g.Where(a => a.Mode == "Keluar").OrderByDescending(a => a.Timestamp).FirstOrDefault();

                    double totalHours = 0;
                    if (Masuk != null && Keluar != null)
                    {
                        totalHours = (Keluar.Timestamp - Masuk.Timestamp).TotalHours;
                    }

                    return new { g.Key.NIM, TotalHours = totalHours };
                })
                .GroupBy(x => x.NIM)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalHours));

            ViewBag.AttendanceCount = attendanceCount;
            ViewBag.TotalHoursPerStudent = attendanceGrouped;
            ViewBag.StudentData = studentData;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View();
        }

        // 🔹 Export Data Laporan ke Excel
        public IActionResult ExportToExcel()
        {
            var attendanceList = _context.Attendances.ToList();
            var studentData = LoadStudentData();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Laporan Kehadiran");
                worksheet.Cell(1, 1).Value = "NIM";
                worksheet.Cell(1, 2).Value = "Nama Mahasiswa";
                worksheet.Cell(1, 3).Value = "Jumlah Kehadiran";
                worksheet.Cell(1, 4).Value = "Total Jam Kehadiran";

                var attendanceCount = attendanceList
                    .Where(a => a.Mode == "Keluar")
                    .GroupBy(a => a.NIM)
                    .ToDictionary(g => g.Key, g => g.Count());

                var attendanceGrouped = attendanceList
                    .GroupBy(a => new { a.NIM, a.Timestamp.Date })
                    .Select(g =>
                    {
                        var Masuk = g.Where(a => a.Mode == "Masuk").OrderBy(a => a.Timestamp).FirstOrDefault();
                        var Keluar = g.Where(a => a.Mode == "Keluar").OrderByDescending(a => a.Timestamp).FirstOrDefault();

                        double totalHours = 0;
                        if (Masuk != null && Keluar != null)
                        {
                            totalHours = (Keluar.Timestamp - Masuk.Timestamp).TotalHours;
                        }

                        return new { g.Key.NIM, TotalHours = totalHours };
                    })
                    .GroupBy(x => x.NIM)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalHours));

                int row = 2;
                foreach (var item in attendanceCount)
                {
                    worksheet.Cell(row, 1).Value = item.Key;
                    worksheet.Cell(row, 2).Value = studentData.ContainsKey(item.Key) ? studentData[item.Key] : "Tidak Diketahui";
                    worksheet.Cell(row, 3).Value = item.Value;
                    worksheet.Cell(row, 4).Value = attendanceGrouped.ContainsKey(item.Key) ? attendanceGrouped[item.Key].ToString("0.00") : "0.00";
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Laporan_Kehadiran.xlsx");
                }
            }
        }

        // 🔹 Halaman Admin untuk melihat data absensi
        public IActionResult Admin(string searchNim, string searchName, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            var studentData = LoadStudentData();
            var data = _context.Attendances.AsQueryable();

            if (!string.IsNullOrEmpty(searchNim))
            {
                data = data.Where(a => a.NIM == searchNim);
            }

            if (!string.IsNullOrEmpty(searchName))
            {
                var matchedNims = studentData
                    .Where(s => s.Value.Contains(searchName, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Key)
                    .ToList();

                data = data.Where(a => matchedNims.Contains(a.NIM));
            }

            var viewModel = data
                .Select(a => new AttendanceViewModel
                {
                    Id = a.Id,
                    NIM = a.NIM,
                    Mode = a.Mode,
                    Timestamp = a.Timestamp,
                    StudentName = studentData.ContainsKey(a.NIM) ? studentData[a.NIM] : "Tidak Diketahui"
                })
                .OrderByDescending(a => a.Timestamp)
                .ToPagedList(page, 10);  // Pastikan Anda menggunakan ToPagedList di sini

            return View(viewModel);
        }

        // 🔹 Hapus Data Absensi
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var attendance = _context.Attendances.Find(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                _context.SaveChanges();
                TempData["Message"] = "Data berhasil dihapus!";
            }
            return RedirectToAction("Admin");
        }
    }
}
