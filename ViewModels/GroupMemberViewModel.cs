using Learning_Management_System.Models;
using System;
using System.ComponentModel;

namespace Learning_Management_System.ViewModels
{
    /// <summary>
    /// ViewModel wrapper for Enrollment that exposes Student info and IndividualRate for editing
    /// </summary>
    public class GroupMemberViewModel : ViewModelBase
    {
        private readonly Enrollment _enrollment;

        public GroupMemberViewModel(Enrollment enrollment)
        {
            _enrollment = enrollment ?? throw new ArgumentNullException(nameof(enrollment));
        }

        public Enrollment Enrollment => _enrollment;
        public Student Student => _enrollment.Student;
        public int StudentId => _enrollment.StudentId;
        public string FullName => _enrollment.Student?.FullName ?? string.Empty;

        public decimal? IndividualRate
        {
            get => _enrollment.IndividualRate;
            set
            {
                if (_enrollment.IndividualRate == value) return;
                _enrollment.IndividualRate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EnrollmentDate => _enrollment.EnrollmentDate;
    }
}
