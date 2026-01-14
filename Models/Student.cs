using System;
using System.Collections.Generic;
using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class Student : ViewModelBase
    {
        public int Id { get; set; }

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName == value) return;
                _firstName = value;
                OnPropertyChanged(); // Tylko powiadomienie o polu - IsDirty zadziała, lista czeka
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName == value) return;
                _lastName = value;
                OnPropertyChanged(); // Tylko powiadomienie o polu - IsDirty zadziała, lista czeka
            }
        }

        private string? _phoneNumber;
        public string? PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }

        private string? _email;
        public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime DateOfBirth { get; set; }
        public bool IsDeleted { get; set; } = false;

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}