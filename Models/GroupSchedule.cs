using System;
using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class GroupSchedule : ViewModelBase
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;

        private DayOfWeek _dayOfWeek;
        public DayOfWeek DayOfWeek
        {
            get => _dayOfWeek;
            set { _dayOfWeek = value; OnPropertyChanged(); }
        }

        private DateTime _startTime = DateTime.Today.AddHours(16);
        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(); }
        }

        private DateTime _endTime = DateTime.Today.AddHours(17);
        public DateTime EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public bool IsDeleted { get; set; } = false;
    }
}