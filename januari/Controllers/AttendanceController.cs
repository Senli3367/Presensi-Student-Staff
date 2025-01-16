using LibraryAttendance.Models;
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

        // Halaman Admin
        public IActionResult Admin(string searchNim)
        {
            var data = _context.Attendances.AsQueryable();

            // Nama Mahasiswa Default
            ViewBag.StudentName = string.Empty;

            if (!string.IsNullOrEmpty(searchNim))
            {
                // Filter data berdasarkan NIM
                data = data.Where(a => a.NIM == searchNim);

                // Tetapkan nama berdasarkan pencocokan NIM
                if (_studentData.TryGetValue(searchNim, out var studentName))
                {
                    ViewBag.StudentName = studentName;
                }
                else
                {
                    ViewBag.StudentName = "Tidak Diketahui";
                }
            }

            return View(data.ToList());
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
