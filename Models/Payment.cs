using System;

namespace Learning_Management_System.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;


        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Description { get; set; }
    }
}
