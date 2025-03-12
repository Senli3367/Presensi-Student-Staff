using System.ComponentModel.DataAnnotations;

namespace LibraryAttendance.Models
{
    public class Student
    {
        [Key]
        public string NIM { get; set; }  // Primary Key
        public string Name { get; set; }
    }
}
