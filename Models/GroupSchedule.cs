using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning_Management_System.Models
{
    public class GroupSchedule
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
