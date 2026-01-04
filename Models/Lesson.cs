using System;
using System.Collections.Generic;
using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class Lesson : ViewModelBase
    {
        public int Id { get; set; }

        // POPRAWIONE: Teraz zmiana GroupId powiadomi ViewModel i aktywuje przycisk Zapisz
        private int _groupId;
        public int GroupId
        {
            get => _groupId;
            set
            {
                if (_groupId == value) return;
                _groupId = value;
                OnPropertyChanged();
            }
        }

        public virtual Group Group { get; set; } = null!;

        public int? GroupScheduleId { get; set; }
        public virtual GroupSchedule? GroupSchedule { get; set; }

        private DateTime _startTime;
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                if (_startTime == value) return;
                _startTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeRange));
            }
        }

        private DateTime _endTime;
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                if (_endTime == value) return;
                _endTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimeRange));
            }
        }

        private string? _note = string.Empty;
        public string? Note
        {
            get => _note;
            set
            {
                if (_note == value) return;
                _note = value;
                OnPropertyChanged();
            }
        }

        private LessonStatus _status;
        public LessonStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";

        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}