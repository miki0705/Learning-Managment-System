using System;
using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class Payment : ViewModelBase
    {
        private int _id;
        public int Id
        {
            get => _id;
            set { if (_id == value) return; _id = value; OnPropertyChanged(); }
        }

        private int _studentId;
        public int StudentId
        {
            get => _studentId;
            set { if (_studentId == value) return; _studentId = value; OnPropertyChanged(); }
        }

        public virtual Student Student { get; set; } = null!;

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { if (_amount == value) return; _amount = value; OnPropertyChanged(); }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set { if (_date == value) return; _date = value; OnPropertyChanged(); }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { if (_description == value) return; _description = value; OnPropertyChanged(); }
        }

        public bool IsDeleted { get; set; } = false;
    }
}
