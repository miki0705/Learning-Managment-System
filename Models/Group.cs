using System;
using System.Collections.Generic;
using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class Group : ViewModelBase
    {
        public int Id { get; set; }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _level = string.Empty;
        public string Level
        {
            get => _level;
            set
            {
                if (_level == value) return;
                _level = value;
                OnPropertyChanged();
            }
        }

        private decimal _baseRate;
        public decimal BaseRate
        {
            get => _baseRate;
            set
            {
                if (_baseRate == value) return;
                _baseRate = value;
                OnPropertyChanged();
            }
        }

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<GroupSchedule> Schedules { get; set; } = new List<GroupSchedule>();
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}