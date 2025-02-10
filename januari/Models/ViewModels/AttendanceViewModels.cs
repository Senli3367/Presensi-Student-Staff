namespace LibraryAttendance.Models.ViewModels
{
    public class AttendanceViewModel
    {
        public int Id { get; set; }
        public string NIM { get; set; } = string.Empty;
        public string StudentName { get; set; } = "Tidak Diketahui"; // Default jika nama tidak ditemukan
        public string Mode { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
