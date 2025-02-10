using LibraryAttendance.Models;
using LibraryAttendance.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAttendance.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;

        // Dictionary untuk menyimpan data mahasiswa (NIM dan Nama)
        private readonly Dictionary<string, string> _studentData = new()
        {
            { "210325965", "Fingky Warni Lastria Simbolon" },
            { "210326254", "Gregorius Vic Vanes Dwi Nanda" },
            { "210326438", "Ehud Nataniel Purba" },
            { "210514115", "Anceline Massora" },
            { "210514399", "Gabriella Dame Octavia Girsang" },
            { "210611121", "Cornelia Shania Endhita Prabowo" },
            { "211126439", "Vincentius Satrio Aryo Setyaki" },
            { "211711469", "Yosef Baptista De Morin Dasman" },
            { "210425999", "Enrico Oktor Narendra" },
            { "211126564", "Jessica Aprilia Stephani Parapat" },
            { "210611119", "Lidia Kurniasih" },
            { "211125935", "Rizki Dewi Antika" },
            { "200710729", "Bernadia Yovita Tiara Sambodo" },
            { "200710829", "Octa Dian Kristanti Kainakaimu" },
            { "210325954", "Marcella Putrika Cankta Dvuti" },
            { "210326268", "Stefanus Sigit Prayoga" },
            { "210426497", "Rosjavsi Weninta Br Barus" }
        };

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // Halaman Absensi (Umum)
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string nim, string mode)
        {
            if (!string.IsNullOrEmpty(nim) && !string.IsNullOrEmpty(mode))
            {
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

        //HALAMAN LAPORAN
        public IActionResult Laporan(DateTime? startDate, DateTime? endDate)
        {
            var data = _context.Attendances.AsQueryable();

            // Filter berdasarkan rentang tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                data = data.Where(a => a.Timestamp >= startDate.Value.Date && a.Timestamp < endDate.Value.Date.AddDays(1));
            }
            else if (startDate.HasValue) // Jika hanya memilih 1 hari
            {
                data = data.Where(a => a.Timestamp.Date == startDate.Value.Date);
            }

            // Eksekusi LINQ ke SQL terlebih dahulu dengan ToList()
            var attendanceList = data.ToList();

            // Hitung total jumlah kehadiran per mahasiswa (Datang + Pergi)
            var attendanceCount = attendanceList
                .GroupBy(a => a.NIM)
                .ToDictionary(g => g.Key, g => g.Count());

            // Hitung total jam kehadiran per mahasiswa dalam rentang tanggal
            var attendanceGrouped = attendanceList
                .GroupBy(a => new { a.NIM, a.Timestamp.Date }) // Kelompokkan berdasarkan NIM dan tanggal
                .Select(g =>
                {
                    var datang = g.Where(a => a.Mode == "Datang").OrderBy(a => a.Timestamp).FirstOrDefault();
                    var pergi = g.Where(a => a.Mode == "Pergi").OrderByDescending(a => a.Timestamp).FirstOrDefault();

                    double totalHours = 0;
                    if (datang != null && pergi != null)
                    {
                        totalHours = (pergi.Timestamp - datang.Timestamp).TotalHours; // Hitung selisih jam
                    }

                    return new
                    {
                        g.Key.NIM,
                        TotalHours = totalHours
                    };
                })
                .GroupBy(x => x.NIM) // Kelompokkan kembali berdasarkan NIM
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalHours)); // Total jam kehadiran untuk setiap mahasiswa

            // Kirimkan data ke ViewBag agar bisa digunakan di View
            ViewBag.AttendanceCount = attendanceCount;
            ViewBag.TotalHoursPerStudent = attendanceGrouped;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View();
        }




        // Halaman Admin dengan Filter Tanggal
        public IActionResult Admin(string searchNim, DateTime? startDate, DateTime? endDate)
        {
            var data = _context.Attendances.AsQueryable();

            // Filter berdasarkan NIM jika diisi
            if (!string.IsNullOrEmpty(searchNim))
            {
                data = data.Where(a => a.NIM == searchNim);
            }

            // Filter berdasarkan rentang tanggal
            if (startDate.HasValue && endDate.HasValue)
            {
                data = data.Where(a => a.Timestamp >= startDate.Value.Date && a.Timestamp < endDate.Value.Date.AddDays(1));
            }
            else if (startDate.HasValue) // Jika hanya memilih 1 hari
            {
                data = data.Where(a => a.Timestamp.Date == startDate.Value.Date);
            }

            // Konversi ke ViewModel untuk menambahkan Nama Mahasiswa
            var viewModel = data.ToList().Select(a => new AttendanceViewModel
            {
                Id = a.Id,
                NIM = a.NIM,
                Mode = a.Mode,
                Timestamp = a.Timestamp,
                StudentName = _studentData.ContainsKey(a.NIM) ? _studentData[a.NIM] : "Tidak Diketahui"
            }).ToList();

            // Kirimkan nilai pencarian ke ViewBag agar tetap terlihat di input
            ViewBag.SearchNim = searchNim;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            var attendance = _context.Attendances.Find(id);
            if (attendance != null)
            {
                _context.Attendances.Remove(attendance);
                _context.SaveChanges();
            }
            return RedirectToAction("Admin");
        }
    }
}
