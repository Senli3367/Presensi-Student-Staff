using LibraryAttendance.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAttendance.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;

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

            // Default nama jika NIM tidak ditemukan
            ViewBag.StudentName = string.Empty;

            if (!string.IsNullOrEmpty(searchNim))
            {
                // Filter data berdasarkan NIM
                data = data.Where(a => a.NIM == searchNim);

                // Tetapkan nama berdasarkan NIM
                if (searchNim == "20113367")
                {
                    ViewBag.StudentName = "Senli";
                }
                else if (searchNim == "20113368")
                {
                    ViewBag.StudentName = "Joko";
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
