namespace LibraryAttendance.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public required string NIM { get; set; }
        public DateTime Timestamp { get; set; }
        public required string Mode { get; set; } // "Datang" atau "Pergi"
    }
}
