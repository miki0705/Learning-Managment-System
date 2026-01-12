using Learning_Management_System.Models;
using Learning_Management_System.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;

namespace Learning_Management_System.ViewModels
{
    public class LessonBrowserViewModel : ViewModelBase
    {
        private readonly ILessonService _lessonService;
        private readonly IAttendanceService _attendanceService;
        private readonly IGroupService _groupService;
        private readonly IPaymentService _paymentService;

        private Lesson? _selectedLesson;
        private Group? _selectedGroupFilter;
        private LessonStatus? _selectedStatusFilter;
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;
        private bool _showMissingAttendanceOnly;
        private bool _isDirty;
        private bool _isInternalUpdate = false;

        public LessonBrowserViewModel(ILessonService lessonService, IAttendanceService attendanceService, 
            IGroupService groupService, IPaymentService paymentService)
        {
            _lessonService = lessonService;
            _attendanceService = attendanceService;
            _groupService = groupService;
            _paymentService = paymentService;

            Lessons = new ObservableCollection<Lesson>();
            Groups = new ObservableCollection<Group>();
            // First add null to represent "All" groups
            Groups.Add(null!);
            // Then add all actual groups
            foreach (var group in _groupService.GetAllGroups())
            {
                Groups.Add(group);
            }
            SelectedLessonAttendances = new ObservableCollection<AttendanceViewModel>();
            AllStatuses = Enum.GetValues(typeof(LessonStatus)).Cast<LessonStatus>().ToList();
            AllAttendanceStatuses = Enum.GetValues(typeof(AttendanceStatus)).Cast<AttendanceStatus>().ToList();

            LoadLessonsCommand = new RelayCommand(async o => await LoadLessonsAsync());
            SelectLessonCommand = new RelayCommand(o => SelectLesson(o as Lesson));
            CancelAttendancesCommand = new RelayCommand(o => CancelAttendancesChanges(), o => IsDirty && SelectedLesson != null);
            CompleteLessonReportCommand = new RelayCommand(async o => await CompleteLessonReportAsync(), o => SelectedLesson != null && CanCompleteReport());
            ToggleFiltersPopupCommand = new RelayCommand(o => IsFiltersPopupVisible = !IsFiltersPopupVisible);

            // Set default date range to last 30 days and next 30 days
            FilterStartDate = DateTime.Today.AddDays(-30);
            FilterEndDate = DateTime.Today.AddDays(30);
            
            // Set default to "All" (null)
            SelectedGroupFilter = null;

            _ = LoadLessonsAsync();
        }

        public ObservableCollection<Lesson> Lessons { get; set; }
        public ObservableCollection<Group> Groups { get; set; }
        
        private bool _isFiltersPopupVisible;
        public bool IsFiltersPopupVisible
        {
            get => _isFiltersPopupVisible;
            set
            {
                if (_isFiltersPopupVisible == value) return;
                _isFiltersPopupVisible = value;
                OnPropertyChanged();
            }
        }
        
        public ICommand ToggleFiltersPopupCommand { get; }
        public ObservableCollection<AttendanceViewModel> SelectedLessonAttendances { get; set; }
        public List<LessonStatus> AllStatuses { get; set; }
        public List<AttendanceStatus> AllAttendanceStatuses { get; set; }

        public Lesson? SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                if (_selectedLesson == value) return;
                if (_selectedLesson != null)
                {
                    _selectedLesson.PropertyChanged -= OnLessonPropertyChanged;
                }
                _selectedLesson = value;
                if (_selectedLesson != null)
                {
                    _selectedLesson.PropertyChanged += OnLessonPropertyChanged;
                }
                OnPropertyChanged();
                _ = LoadAttendancesForLessonAsync();
                OnPropertyChanged(nameof(HasMissingAttendance));
                IsDirty = false;
            }
        }

        private void OnLessonPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            if (e.PropertyName == nameof(Lesson.Status))
            {
                IsDirty = true;
            }
        }

        public Group? SelectedGroupFilter
        {
            get => _selectedGroupFilter;
            set
            {
                if (_selectedGroupFilter == value) return;
                _selectedGroupFilter = value;
                OnPropertyChanged();
                _ = LoadLessonsAsync();
            }
        }

        public LessonStatus? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (_selectedStatusFilter == value) return;
                _selectedStatusFilter = value;
                OnPropertyChanged();
                _ = LoadLessonsAsync();
            }
        }

        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set
            {
                if (_filterStartDate == value) return;
                _filterStartDate = value;
                OnPropertyChanged();
                _ = LoadLessonsAsync();
            }
        }

        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set
            {
                if (_filterEndDate == value) return;
                _filterEndDate = value;
                OnPropertyChanged();
                _ = LoadLessonsAsync();
            }
        }

        public bool ShowMissingAttendanceOnly
        {
            get => _showMissingAttendanceOnly;
            set
            {
                if (_showMissingAttendanceOnly == value) return;
                _showMissingAttendanceOnly = value;
                OnPropertyChanged();
                _ = LoadLessonsAsync();
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value) return;
                _isDirty = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBlocked));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsBlocked => IsDirty;

        public bool HasMissingAttendance
        {
            get
            {
                if (SelectedLesson == null) return false;
                var enrolledCount = SelectedLesson.Group?.Enrollments?.Count ?? 0;
                var attendanceCount = SelectedLessonAttendances.Count;
                return enrolledCount > attendanceCount;
            }
        }

        public ICommand LoadLessonsCommand { get; }
        public ICommand SelectLessonCommand { get; }
        public ICommand CancelAttendancesCommand { get; }
        public ICommand CompleteLessonReportCommand { get; }

        public async Task LoadLessonsAsync()
        {
            var startDate = FilterStartDate ?? DateTime.MinValue;
            var endDate = FilterEndDate ?? DateTime.MaxValue;

            var lessons = await _lessonService.GetLessonsForDateRangeAsync(startDate, endDate);

            // Apply filters
            // SelectedGroupFilter is null when "All" is selected
            if (SelectedGroupFilter != null)
            {
                lessons = lessons.Where(l => l.GroupId == SelectedGroupFilter.Id);
            }

            if (SelectedStatusFilter.HasValue)
            {
                lessons = lessons.Where(l => l.Status == SelectedStatusFilter.Value);
            }

            if (ShowMissingAttendanceOnly)
            {
                var lessonsWithoutAttendance = await _attendanceService.GetLessonsWithoutAttendanceAsync();
                var missingIds = lessonsWithoutAttendance.Select(l => l.Id).ToHashSet();
                lessons = lessons.Where(l => missingIds.Contains(l.Id));
            }

            Lessons.Clear();
            foreach (var lesson in lessons.OrderByDescending(l => l.StartTime))
            {
                Lessons.Add(lesson);
            }

        }

        private void SelectLesson(Lesson? lesson)
        {
            SelectedLesson = lesson;
        }

        private async Task LoadAttendancesForLessonAsync()
        {
            SelectedLessonAttendances.Clear();
            IsDirty = false;

            if (SelectedLesson == null || SelectedLesson.Group == null)
                return;

            // Get existing attendances
            var existingAttendances = await _attendanceService.GetAttendancesByLessonAsync(SelectedLesson.Id);
            var existingStudentIds = existingAttendances.Select(a => a.StudentId).ToHashSet();

            // Get all enrolled students
            var enrolledStudents = SelectedLesson.Group.Enrollments.Select(e => e.Student).ToList();

            // Create attendance view models for all enrolled students
            foreach (var student in enrolledStudents)
            {
                var existingAttendance = existingAttendances.FirstOrDefault(a => a.StudentId == student.Id);
                
                var attendanceVM = new AttendanceViewModel
                {
                    Student = student,
                    Lesson = SelectedLesson,
                    Attendance = existingAttendance ?? new Attendance
                    {
                        LessonId = SelectedLesson.Id,
                        StudentId = student.Id,
                        Status = AttendanceStatus.None,
                        PriceCharged = 0
                    }
                };

                attendanceVM.PropertyChanged += OnAttendancePropertyChanged;
                SelectedLessonAttendances.Add(attendanceVM);
            }

            // Calculate prices for all attendances
            CalculateAttendancePrices();
        }

        private void CalculateAttendancePrices()
        {
            if (SelectedLesson == null || SelectedLesson.Group == null)
                return;

            foreach (var attendanceVM in SelectedLessonAttendances)
            {
                decimal price = 0;

                if (attendanceVM.Status == AttendanceStatus.Present || attendanceVM.Status == AttendanceStatus.AbsentPaid)
                {
                    // Get enrollment to check for individual rate
                    var enrollment = SelectedLesson.Group.Enrollments
                        .FirstOrDefault(e => e.StudentId == attendanceVM.Student.Id);

                    if (enrollment?.IndividualRate.HasValue == true)
                    {
                        price = enrollment.IndividualRate.Value;
                    }
                    else
                    {
                        price = SelectedLesson.Group.BaseRate;
                    }
                }
                // None, AbsentFree and Late = 0

                attendanceVM.PriceCharged = price;
            }
        }

        private void OnAttendancePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            
            if (e.PropertyName == nameof(AttendanceViewModel.Status))
            {
                CalculateAttendancePrices();
            }
            
            IsDirty = true;
        }

        private async Task SaveAttendancesAsync()
        {
            if (SelectedLesson == null) return;

            var attendances = SelectedLessonAttendances
                .Select(avm => avm.Attendance)
                .ToList();

            await _attendanceService.BulkUpdateAttendancesAsync(SelectedLesson.Id, attendances);
            IsDirty = false;
            await LoadAttendancesForLessonAsync();
        }

        private void CancelAttendancesChanges()
        {
            _ = LoadAttendancesForLessonAsync();
        }

        private bool CanCompleteReport()
        {
            if (SelectedLesson == null) return false;
            
            // If lesson is Holiday or Canceled, allow completion without attendance requirements
            if (SelectedLesson.Status == LessonStatus.Holiday || SelectedLesson.Status == LessonStatus.Canceled)
            {
                return true;
            }
            
            // For other statuses, check if all enrolled students have attendance with valid status (not None)
            var enrolledCount = SelectedLesson.Group?.Enrollments?.Count ?? 0;
            
            // All students should have attendance records with valid status (not None)
            return enrolledCount == SelectedLessonAttendances.Count && 
                   SelectedLessonAttendances.All(a => a.Status != AttendanceStatus.None);
        }

        private async Task CompleteLessonReportAsync()
        {
            if (SelectedLesson == null) return;

            // For Holiday or Canceled status, allow completion without attendance validation
            bool isHolidayOrCanceled = SelectedLesson.Status == LessonStatus.Holiday || SelectedLesson.Status == LessonStatus.Canceled;
            
            // Check if all students have attendance set (not None)
            bool allAttendancesSet = CanCompleteReport();
            
            // If all attendances are set and status is not Holiday/Canceled, automatically set to Completed
            if (allAttendancesSet && !isHolidayOrCanceled)
            {
                SelectedLesson.Status = LessonStatus.Completed;
            }
            
            // Validate all students have attendance (not None) only if not Holiday/Canceled
            if (!isHolidayOrCanceled && !allAttendancesSet)
            {
                MessageBox.Show("Wszyscy uczniowie muszą mieć uzupełnioną frekwencję przed zakończeniem raportu lekcji.", 
                    "Brakujące dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Save attendances
            await SaveAttendancesAsync();

            // Save lesson status along with attendances
            await _lessonService.UpdateLessonAsync(SelectedLesson);

            // Update wallets from lesson (only if lesson is completed)
            if (SelectedLesson.Status == LessonStatus.Completed)
            {
                await _paymentService.UpdateWalletsFromLessonAsync(SelectedLesson.Id);
            }

            // Reload lesson to refresh UI
            await LoadLessonsAsync();
            
            // Reselect the lesson to refresh UI
            var lessonId = SelectedLesson.Id;
            SelectedLesson = null;
            SelectedLesson = Lessons.FirstOrDefault(l => l.Id == lessonId);

            MessageBox.Show("Raport lekcji został ukończony. Portfele uczniów zostały zaktualizowane.", 
                "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class AttendanceViewModel : ViewModelBase
    {
        private Student _student = null!;
        private Lesson _lesson = null!;
        private Attendance _attendance = null!;
        private AttendanceStatus _status;
        private decimal _priceCharged;

        public Student Student
        {
            get => _student;
            set
            {
                if (_student == value) return;
                _student = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StudentName));
            }
        }

        public string StudentName => Student?.FullName ?? string.Empty;

        public Lesson Lesson
        {
            get => _lesson;
            set
            {
                if (_lesson == value) return;
                _lesson = value;
                OnPropertyChanged();
            }
        }

        public Attendance Attendance
        {
            get => _attendance;
            set
            {
                if (_attendance == value) return;
                _attendance = value;
                if (_attendance != null)
                {
                    Status = _attendance.Status;
                    PriceCharged = _attendance.PriceCharged;
                }
                OnPropertyChanged();
            }
        }

        public AttendanceStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                if (_attendance != null)
                {
                    _attendance.Status = value;
                }
                OnPropertyChanged();
            }
        }

        public decimal PriceCharged
        {
            get => _priceCharged;
            set
            {
                if (_priceCharged == value) return;
                _priceCharged = value;
                if (_attendance != null)
                {
                    _attendance.PriceCharged = value;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriceChargedDisplay));
            }
        }

        public string PriceChargedDisplay => $"{PriceCharged:F2} PLN";
    }
}
