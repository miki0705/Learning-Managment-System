using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Learning_Management_System.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Learning_Management_System.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Pola prywatne
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly ILessonService _lessonService;
        private readonly IPaymentService _paymentService;
        private readonly IAttendanceService _attendanceService;
        private readonly IBackupService _backupService;

        private DateTime _currentWeekStart;
        private Student? _selectedStudent;
        private Group? _selectedGroup;
        private Lesson? _selectedLesson;
        private ObservableCollection<GroupMemberViewModel> _groupMembers;
        private ObservableCollection<GroupSchedule> _groupSchedules;
        private bool _isEditingMembers;
        private bool _isDirty;
        private bool _isInternalUpdate = false;
        private string _searchText = string.Empty;
        private List<StudentSelection> _allStudentsFullList = new();
        #endregion

        #region Właściwości Publiczne
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

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Group> Groups { get; set; }
        public ObservableCollection<DayViewModel> WeekDays { get; set; } = new();
        public ObservableCollection<StudentSelection> AllStudentsSelection { get; set; } = new();

        public IEnumerable<LessonStatus> AllStatuses => Enum.GetValues(typeof(LessonStatus)).Cast<LessonStatus>();

        public ObservableCollection<GroupMemberViewModel> GroupMembers
        {
            get => _groupMembers;
            set { _groupMembers = value; OnPropertyChanged(); }
        }

        public ObservableCollection<GroupSchedule> GroupSchedules
        {
            get => _groupSchedules;
            set { _groupSchedules = value; OnPropertyChanged(); }
        }

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent == value) return;
                if (_selectedStudent != null) _selectedStudent.PropertyChanged -= OnModelPropertyChanged;
                _selectedStudent = value;
                if (_selectedStudent != null) _selectedStudent.PropertyChanged += OnModelPropertyChanged;

                OnPropertyChanged();
                IsDirty = false;
            }
        }

        public Group? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup == value) return;
                if (_selectedGroup != null) _selectedGroup.PropertyChanged -= OnModelPropertyChanged;
                _selectedGroup = value;
                if (_selectedGroup != null) _selectedGroup.PropertyChanged += OnModelPropertyChanged;

                OnPropertyChanged();
                IsEditingMembers = false;
                IsDirty = false;
                LoadGroupMembers();
                LoadGroupSchedules();
            }
        }

        public Lesson? SelectedLesson
        {
            get => _selectedLesson;
            set
            {
                if (_selectedLesson == value) return;
                if (_selectedLesson != null) _selectedLesson.PropertyChanged -= OnModelPropertyChanged;
                _selectedLesson = value;
                if (_selectedLesson != null) _selectedLesson.PropertyChanged += OnModelPropertyChanged;

                OnPropertyChanged();
                IsDirty = false;
            }
        }

        public bool IsEditingMembers
        {
            get => _isEditingMembers;
            set 
            { 
                if (_isEditingMembers == value) return;
                _isEditingMembers = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBlocked));
            }
        }

        /// <summary>
        /// Returns true when the UI should be blocked (tabs and group selection disabled).
        /// This happens when either IsDirty is true, IsEditingMembers is true, 
        /// FinanceViewModel is blocked, or LessonBrowserViewModel is blocked.
        /// </summary>
        public bool IsBlocked => IsDirty || IsEditingMembers || FinanceViewModel.IsBlocked || LessonBrowserViewModel.IsBlocked;

        public string CurrentWeekTitle => $"{_currentWeekStart:dd.MM} - {_currentWeekStart.AddDays(6):dd.MM.yyyy}";

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value) return;
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        private bool _isCalendarWidgetVisible;
        public bool IsCalendarWidgetVisible
        {
            get => _isCalendarWidgetVisible;
            set
            {
                if (_isCalendarWidgetVisible == value) return;
                _isCalendarWidgetVisible = value;
                OnPropertyChanged();
                if (value)
                {
                    // Set display date to current week start when opening
                    CalendarDisplayDate = _currentWeekStart;
                }
                else
                {
                    // Clear selected date when hiding
                    SelectedDate = null;
                }
            }
        }

        private DateTime _calendarDisplayDate = DateTime.Today;
        public DateTime CalendarDisplayDate
        {
            get => _calendarDisplayDate;
            set
            {
                if (_calendarDisplayDate == value) return;
                _calendarDisplayDate = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Komendy
        public ICommand AddStudentCommand { get; }
        public ICommand SaveStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand CancelStudentCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand SaveGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand CancelGroupCommand { get; }
        public ICommand StartEditMembersCommand { get; }
        public ICommand SaveMembersCommand { get; }
        public ICommand CancelEditMembersCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand PreviousWeekCommand { get; }
        public ICommand JumpToWeekCommand { get; }
        public ICommand ToggleCalendarWidgetCommand { get; }
        public ICommand SelectLessonCommand { get; }
        public ICommand DeleteLessonCommand { get; }
        public ICommand SaveLessonCommand { get; }
        public ICommand CancelLessonCommand { get; }
        public ICommand AddManualLessonCommand { get; }
        #endregion

        public FinanceViewModel FinanceViewModel { get; }
        public LessonBrowserViewModel LessonBrowserViewModel { get; }
        public ReportViewModel ReportViewModel { get; }

        public ICommand BackupDatabaseCommand { get; }
        public ICommand RestoreDatabaseCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand ImportDataCommand { get; }

        public MainViewModel(IStudentService studentService, IGroupService groupService, ILessonService lessonService, 
            IPaymentService paymentService, IAttendanceService attendanceService,
            IReportService reportService, IExportService exportService, IBackupService backupService)
        {
            _studentService = studentService;
            _groupService = groupService;
            _lessonService = lessonService;
            _paymentService = paymentService;
            _attendanceService = attendanceService;
            _backupService = backupService;

            FinanceViewModel = new FinanceViewModel(paymentService, studentService);
            LessonBrowserViewModel = new LessonBrowserViewModel(lessonService, attendanceService, groupService, paymentService);
            ReportViewModel = new ReportViewModel(reportService, exportService);

            // Subscribe to PropertyChanged events to update IsBlocked when child ViewModels' IsBlocked changes
            FinanceViewModel.PropertyChanged += OnChildViewModelPropertyChanged;
            LessonBrowserViewModel.PropertyChanged += OnChildViewModelPropertyChanged;

            BackupDatabaseCommand = new RelayCommand(async o => await BackupDatabaseAsync());
            RestoreDatabaseCommand = new RelayCommand(async o => await RestoreDatabaseAsync());
            ExportDataCommand = new RelayCommand(async o => await ExportDataAsync());
            ImportDataCommand = new RelayCommand(async o => await ImportDataAsync());

            Students = new ObservableCollection<Student>(_studentService.GetAllStudents());
            Groups = new ObservableCollection<Group>(_groupService.GetAllGroups());
            _groupMembers = new ObservableCollection<GroupMemberViewModel>();
            _groupSchedules = new ObservableCollection<GroupSchedule>();

            // Inicjalizacja Komend
            AddStudentCommand = new RelayCommand(o => AddStudent(), o => !IsDirty);
            SaveStudentCommand = new RelayCommand(o => SaveStudent(), o => IsDirty);
            DeleteStudentCommand = new RelayCommand(o => DeleteStudent(), o => SelectedStudent != null && !IsDirty);
            CancelStudentCommand = new RelayCommand(o => CancelStudentChanges(), o => IsDirty && SelectedStudent != null);

            AddGroupCommand = new RelayCommand(o => AddGroup(), o => !IsBlocked);
            SaveGroupCommand = new RelayCommand(async o => await SaveGroupAsync(), o => IsDirty);
            DeleteGroupCommand = new RelayCommand(async o => await DeleteGroupAsync(), o => SelectedGroup != null && !IsBlocked);
            CancelGroupCommand = new RelayCommand(o => CancelGroupChanges(), o => IsDirty && SelectedGroup != null);

            StartEditMembersCommand = new RelayCommand(o => StartEditMembers(), o => SelectedGroup != null);
            SaveMembersCommand = new RelayCommand(async o => await SaveMembersAsync());
            CancelEditMembersCommand = new RelayCommand(o => { IsEditingMembers = false; });

            AddScheduleCommand = new RelayCommand(o => AddSchedule(), o => SelectedGroup != null);
            DeleteScheduleCommand = new RelayCommand(async o => await DeleteScheduleAsync(o as GroupSchedule), o => o is GroupSchedule);

            NextWeekCommand = new RelayCommand(async o => await ChangeWeekAsync(7));
            PreviousWeekCommand = new RelayCommand(async o => await ChangeWeekAsync(-7));
            JumpToWeekCommand = new RelayCommand(async o => await JumpToWeekAsync(), o => SelectedDate.HasValue);
            ToggleCalendarWidgetCommand = new RelayCommand(o => IsCalendarWidgetVisible = !IsCalendarWidgetVisible);

            SelectLessonCommand = new RelayCommand(o => {
                if (o is Lesson lesson) SelectedLesson = lesson;
                else SelectedLesson = null;
            });

            DeleteLessonCommand = new RelayCommand(async o => await DeleteLessonAsync(), o => SelectedLesson != null);
            SaveLessonCommand = new RelayCommand(async o => await SaveLessonAsync(), o => IsDirty && SelectedLesson != null);
            CancelLessonCommand = new RelayCommand(async o => await CancelLessonChangesAsync(), o => IsDirty && SelectedLesson != null);
            AddManualLessonCommand = new RelayCommand(o => AddManualLesson());

            SetInitialWeek();
        }

        #region Metody Pomocnicze
        private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            IsDirty = true;
        }

        private void OnSchedulePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            IsDirty = true;
        }

        private void OnChildViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When IsBlocked or IsDirty changes in child ViewModels, notify that MainViewModel.IsBlocked has changed
            if (e.PropertyName == nameof(FinanceViewModel.IsBlocked) || 
                e.PropertyName == nameof(FinanceViewModel.IsDirty) ||
                e.PropertyName == nameof(LessonBrowserViewModel.IsBlocked) || 
                e.PropertyName == nameof(LessonBrowserViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsBlocked));
            }
        }

        private void RefreshList()
        {
            CollectionViewSource.GetDefaultView(Students)?.Refresh();
            CollectionViewSource.GetDefaultView(Groups)?.Refresh();
        }

        private void ApplyFilter()
        {
            // Filtrujemy listę _allStudentsFullList i wynik wrzucamy do AllStudentsSelection (to, co widzi UI)
            var filtered = _allStudentsFullList
                .Where(s => string.IsNullOrWhiteSpace(SearchText) ||
                            s.Student.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            AllStudentsSelection.Clear();
            foreach (var item in filtered)
            {
                AllStudentsSelection.Add(item);
            }
        }
        #endregion

        #region Logika UCZNIÓW
        private void AddStudent()
        {
            var newStudent = _studentService.CreateNewStudent();
            Students.Add(newStudent);
            SelectedStudent = newStudent;
            IsDirty = true;
        }

        private void SaveStudent()
        {
            if (SelectedStudent == null) return;
            if (SelectedStudent.Id == 0) _studentService.AddStudent(SelectedStudent);
            _studentService.SaveChanges();
            IsDirty = false;
            
            // Reload Students collection from database
            var allStudents = _studentService.GetAllStudents().ToList();
            Students.Clear();
            foreach (var student in allStudents)
            {
                Students.Add(student);
            }
            
            // Refresh FinanceViewModel students list
            FinanceViewModel.RefreshStudents();
            
            // If currently editing group members, refresh the student selection list
            if (IsEditingMembers)
            {
                RefreshGroupMemberSelection();
            }
            
            RefreshList();
        }

        private void CancelStudentChanges()
        {
            if (SelectedStudent == null) return;
            _isInternalUpdate = true;
            if (_studentService.IsNew(SelectedStudent))
            {
                Students.Remove(SelectedStudent);
                SelectedStudent = null;
            }
            else
            {
                _studentService.ReloadStudent(SelectedStudent);
                var temp = SelectedStudent;
                SelectedStudent = null;
                SelectedStudent = temp;
            }
            _isInternalUpdate = false;
            IsDirty = false;
            RefreshList();
        }

        private void DeleteStudent()
        {
            if (SelectedStudent == null) return;
            if (MessageBox.Show($"Usunąć ucznia {SelectedStudent.FullName}?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedStudent.Id > 0)
                {
                    _studentService.DeleteStudent(SelectedStudent);
                    _studentService.SaveChanges();
                }
                Students.Remove(SelectedStudent);
                SelectedStudent = null;
                IsDirty = false;
                
                // Refresh FinanceViewModel students list
                FinanceViewModel.RefreshStudents();
                
                // If currently editing group members, refresh the student selection list
                if (IsEditingMembers)
                {
                    RefreshGroupMemberSelection();
                }
            }
        }
        #endregion

        #region Logika GRUP
        private void AddGroup()
        {
            var newGroup = _groupService.CreateNewGroup();
            Groups.Add(newGroup);
            SelectedGroup = newGroup;
            IsDirty = true;
        }

        private async Task SaveGroupAsync()
        {
            if (SelectedGroup == null) return;
            if (SelectedGroup.Id == 0) _groupService.AddGroup(SelectedGroup);
            await _groupService.SaveChangesAsync();

            await _lessonService.SyncLessonsWithScheduleAsync(SelectedGroup);

            IsDirty = false;
            RefreshList();
            await LoadWeekDataAsync();
            
            // Refresh lesson browser to show newly synced lessons
            await LessonBrowserViewModel.LoadLessonsAsync();
        }

        private void CancelGroupChanges()
        {
            if (SelectedGroup == null) return;
            _isInternalUpdate = true;
            if (_groupService.IsNew(SelectedGroup))
            {
                Groups.Remove(SelectedGroup);
                SelectedGroup = null;
            }
            else
            {
                _groupService.ReloadGroup(SelectedGroup);
                var temp = SelectedGroup;
                SelectedGroup = null;
                SelectedGroup = temp;
                LoadGroupMembers();
                LoadGroupSchedules();
            }
            _isInternalUpdate = false;
            IsEditingMembers = false;
            IsDirty = false;
            RefreshList();
        }

        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup == null) return;
            if (MessageBox.Show($"Usunąć grupę {SelectedGroup.Name}?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedGroup.Id > 0)
                {
                    _groupService.DeleteGroup(SelectedGroup);
                    await _groupService.SaveChangesAsync();
                }
                Groups.Remove(SelectedGroup);
                SelectedGroup = null;
                IsDirty = false;
            }
        }

        private void LoadGroupMembers()
        {
            if (GroupMembers != null)
            {
                // Unsubscribe from property changes to avoid memory leaks
                foreach (var member in GroupMembers)
                {
                    member.PropertyChanged -= OnGroupMemberPropertyChanged;
                }
            }

            if (SelectedGroup == null || SelectedGroup.Id == 0)
            {
                GroupMembers = new ObservableCollection<GroupMemberViewModel>();
                return;
            }

            var members = SelectedGroup.Enrollments
                .Select(e => new GroupMemberViewModel(e))
                .ToList();

            // Subscribe to property changes to track IsDirty
            foreach (var member in members)
            {
                member.PropertyChanged += OnGroupMemberPropertyChanged;
            }

            GroupMembers = new ObservableCollection<GroupMemberViewModel>(members);
        }

        private void OnGroupMemberPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            if (e.PropertyName == nameof(GroupMemberViewModel.IndividualRate))
            {
                IsDirty = true;
            }
        }

        private void LoadGroupSchedules()
        {
            if (GroupSchedules != null)
                foreach (var s in GroupSchedules) s.PropertyChanged -= OnSchedulePropertyChanged;

            if (SelectedGroup == null)
            {
                GroupSchedules = new ObservableCollection<GroupSchedule>();
                return;
            }

            GroupSchedules = new ObservableCollection<GroupSchedule>(SelectedGroup.Schedules);
            foreach (var schedule in GroupSchedules) schedule.PropertyChanged += OnSchedulePropertyChanged;
        }

        private void AddSchedule()
        {
            if (SelectedGroup == null) return;
            var newSchedule = new GroupSchedule
            {
                GroupId = SelectedGroup.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = DateTime.Today.AddHours(16),
                EndTime = DateTime.Today.AddHours(17).AddMinutes(30)
            };
            newSchedule.PropertyChanged += OnSchedulePropertyChanged;
            SelectedGroup.Schedules.Add(newSchedule);
            GroupSchedules.Add(newSchedule);
            IsDirty = true;
        }

        private async Task DeleteScheduleAsync(GroupSchedule? schedule)
        {
            if (SelectedGroup == null || schedule == null) return;

            var result = MessageBox.Show(
                "Czy na pewno chcesz usunąć ten termin z grafiku?\nSpowoduje to również usunięcie zaplanowanych przyszłych lekcji.",
                "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            schedule.PropertyChanged -= OnSchedulePropertyChanged;

            if (schedule.Id > 0)
            {
                // TUTAJ POPRAWKA: Używamy asynchronicznej metody z serwisu
                var futureLessons = await _lessonService.GetFutureLessonsByScheduleAsync(schedule.Id);

                foreach (var lesson in futureLessons)
                {
                    await _lessonService.DeleteLessonAsync(lesson.Id);
                }
            }

            SelectedGroup.Schedules.Remove(schedule);
            GroupSchedules.Remove(schedule);
            if (SelectedGroup.Id > 0) await _groupService.SaveChangesAsync();

            await LoadWeekDataAsync();
            IsDirty = true;
        }

        private void StartEditMembers()
        {
            if (SelectedGroup == null || SelectedGroup.Id == 0) return;

            _allStudentsFullList.Clear();

            // Pobieramy ID aktualnych członków
            var currentMemberIds = SelectedGroup.Enrollments.Select(e => e.StudentId).ToList();

            foreach (var student in Students)
            {
                // Tworzymy listę pomocniczą do filtrowania
                _allStudentsFullList.Add(new StudentSelection(student, currentMemberIds.Contains(student.Id)));
            }

            _searchText = string.Empty; // Bezpośrednio do pola, by nie wywołać ApplyFilter za wcześnie
            OnPropertyChanged(nameof(SearchText));
            ApplyFilter();

            IsEditingMembers = true;
        }

        private void RefreshGroupMemberSelection()
        {
            if (!IsEditingMembers || SelectedGroup == null || SelectedGroup.Id == 0) return;

            // Preserve current selections
            var selectedIds = _allStudentsFullList.Where(s => s.IsSelected).Select(s => s.Student.Id).ToList();
            
            _allStudentsFullList.Clear();

            // Pobieramy ID aktualnych członków
            var currentMemberIds = SelectedGroup.Enrollments.Select(e => e.StudentId).ToList();

            foreach (var student in Students)
            {
                // Check if this student was previously selected or is a current member
                var isSelected = selectedIds.Contains(student.Id) || currentMemberIds.Contains(student.Id);
                _allStudentsFullList.Add(new StudentSelection(student, isSelected));
            }

            ApplyFilter();
        }

        private async Task SaveMembersAsync()
        {
            if (SelectedGroup == null) return;

            // Pobieramy zaznaczonych studentów Z CAŁEJ LISTY (nie tylko przefiltrowanej!)
            var selectedStudents = _allStudentsFullList
                .Where(s => s.IsSelected)
                .Select(s => s.Student)
                .ToList();

            // Wywołujemy synchronizację w serwisie
            _groupService.UpdateGroupMembers(SelectedGroup, selectedStudents);

            // ZAPISUJEMY ZMIANY DO BAZY
            await _groupService.SaveChangesAsync();

            // Odświeżamy widok UI
            LoadGroupMembers();

            IsEditingMembers = false;
        }
        #endregion

        #region Logika KALENDARZA i LEKCJI
        private void SetInitialWeek()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            _currentWeekStart = today.AddDays(-1 * diff).Date;
            OnPropertyChanged(nameof(CurrentWeekTitle));
            _ = LoadWeekDataAsync();
        }

        private async Task LoadWeekDataAsync()
        {
            var lessons = await _lessonService.GetLessonsForDateRangeAsync(_currentWeekStart, _currentWeekStart.AddDays(7));

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                WeekDays.Clear();
                for (int i = 0; i < 7; i++)
                {
                    var date = _currentWeekStart.AddDays(i);
                    var dayVM = new DayViewModel { Date = date };
                    var dayLessons = lessons.Where(l => l.StartTime.Date == date.Date).OrderBy(l => l.StartTime);
                    foreach (var l in dayLessons) dayVM.Lessons.Add(l);
                    WeekDays.Add(dayVM);
                }
            });
        }

        private async Task ChangeWeekAsync(int days)
        {
            SelectedLesson = null;
            _currentWeekStart = _currentWeekStart.AddDays(days);
            OnPropertyChanged(nameof(CurrentWeekTitle));
            await LoadWeekDataAsync();
        }

        private async Task JumpToWeekAsync()
        {
            if (!SelectedDate.HasValue) return;

            SelectedLesson = null;
            DateTime selectedDate = SelectedDate.Value;
            
            // Calculate the Monday of the week containing the selected date
            int diff = (7 + (selectedDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            _currentWeekStart = selectedDate.AddDays(-1 * diff).Date;
            
            OnPropertyChanged(nameof(CurrentWeekTitle));
            await LoadWeekDataAsync();
            
            // Clear the selected date and hide the calendar widget after jumping
            SelectedDate = null;
            IsCalendarWidgetVisible = false;
        }

        private void AddManualLesson()
        {
            var newLesson = new Lesson
            {
                StartTime = DateTime.Today.AddHours(16),
                EndTime = DateTime.Today.AddHours(17).AddMinutes(30),
                Status = LessonStatus.Scheduled,
                Note = "Nowa lekcja",
                GroupId = SelectedGroup?.Id ?? (Groups.FirstOrDefault()?.Id ?? 0)
            };
            SelectedLesson = newLesson;
            IsDirty = true;
        }

        private async Task SaveLessonAsync()
        {
            if (SelectedLesson == null) return;
            if (SelectedLesson.Id == 0)
                await _lessonService.AddLessonAsync(SelectedLesson);
            else
                await _lessonService.UpdateLessonAsync(SelectedLesson);

            IsDirty = false;
            await LoadWeekDataAsync();
            await LessonBrowserViewModel.LoadLessonsAsync();
        }

        private async Task CancelLessonChangesAsync()
        {
            if (SelectedLesson == null) return;
            _isInternalUpdate = true;
            if (_lessonService.IsNew(SelectedLesson))
            {
                // If it's a new lesson, just remove it from the selection
                SelectedLesson = null;
            }
            else
            {
                // Save the lesson ID before reloading
                int lessonId = SelectedLesson.Id;
                
                // Reload the lesson entity from database to discard changes
                _lessonService.ReloadLesson(SelectedLesson);
                
                // Reload week data to refresh the calendar view
                await LoadWeekDataAsync();
                
                // Find and reselect the lesson from the reloaded collection (if it's in current week)
                var reloadedLesson = WeekDays
                    .SelectMany(d => d.Lessons)
                    .FirstOrDefault(l => l.Id == lessonId);
                
                SelectedLesson = null;
                if (reloadedLesson != null)
                {
                    SelectedLesson = reloadedLesson;
                }
            }
            _isInternalUpdate = false;
            IsDirty = false;
        }

        private async Task DeleteLessonAsync()
        {
            if (SelectedLesson == null) return;
            if (MessageBox.Show("Usunąć tę lekcję?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedLesson.Id > 0)
                    await _lessonService.DeleteLessonAsync(SelectedLesson.Id);
                SelectedLesson = null;
                await LoadWeekDataAsync();
                await LessonBrowserViewModel.LoadLessonsAsync();
            }
        }
        #endregion


        #region Backup Methods
        private async Task BackupDatabaseAsync()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Database files (*.db)|*.db|All files (*.*)|*.*",
                FileName = $"LearningManagementSystem_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    await _backupService.BackupDatabaseAsync(saveDialog.FileName);
                    MessageBox.Show("Kopia zapasowa została utworzona pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas tworzenia kopii zapasowej: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task RestoreDatabaseAsync()
        {
            var result = MessageBox.Show(
                "Przywrócenie bazy danych spowoduje zamknięcie aplikacji. Czy chcesz kontynuować?",
                "Ostrzeżenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Database files (*.db)|*.db|All files (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _backupService.RestoreDatabaseAsync(openDialog.FileName);
                        MessageBox.Show(
                            "Baza danych została przywrócona. Aplikacja zostanie zamknięta. Uruchom ją ponownie.",
                            "Sukces",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas przywracania bazy danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task ExportDataAsync()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"LearningManagementSystem_export_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    await _backupService.ExportToJsonAsync(saveDialog.FileName);
                    MessageBox.Show("Dane zostały wyeksportowane pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas eksportu danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ImportDataAsync()
        {
            var result = MessageBox.Show(
                "Import danych może nadpisać istniejące dane. Czy chcesz kontynuować?",
                "Ostrzeżenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (openDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _backupService.ImportFromJsonAsync(openDialog.FileName);
                        MessageBox.Show(
                            "Dane zostały zaimportowane. Aplikacja zostanie zamknięta. Uruchom ją ponownie.",
                            "Sukces",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Błąd podczas importu danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion
    }
}