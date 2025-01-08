using Microsoft.EntityFrameworkCore;

namespace LibraryAttendance.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Attendance> Attendances { get; set; }
    }
}
