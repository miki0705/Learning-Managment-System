using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning_Management_System.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }

        public decimal? IndividualRate { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
    }
}
