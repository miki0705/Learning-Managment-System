using System;

namespace Learning_Management_System.Models
{
    public class Attendance
    { 
        public int Id { get; set; }
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; } = null!;
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public AttendanceStatus Status { get; set; }
        public decimal PriceCharged {  get; set; }
    }
}
