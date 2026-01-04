using System;

namespace Learning_Management_System.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string? Note { get; set; } = string.Empty;

        public LessonStatus Status { get; set; }


        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
