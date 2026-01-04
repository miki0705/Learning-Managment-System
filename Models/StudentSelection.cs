using Learning_Management_System.ViewModels;

namespace Learning_Management_System.Models
{
    public class StudentSelection : ViewModelBase
    {
        public Student Student { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public StudentSelection(Student student, bool isSelected)
        {
            Student = student;
            _isSelected = isSelected;
        }
    }
}