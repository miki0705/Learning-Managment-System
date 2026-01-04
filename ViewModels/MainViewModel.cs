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

        private DateTime _currentWeekStart;
        private Student? _selectedStudent;
        private Group? _selectedGroup;
        private Lesson? _selectedLesson;
        private ObservableCollection<Student> _groupMembers;
        private ObservableCollection<GroupSchedule> _groupSchedules;
        private bool _isEditingMembers;
        private bool _isDirty;
        private bool _isInternalUpdate = false;
        private string _searchText = string.Empty;
        private List<StudentSelection> _allStudentsFullList = new();
        #endregion

        #region Właściwości Publiczne (Główne)
        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value) return;
                _isDirty = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }
        #endregion

        #region Kolekcje Danych
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Group> Groups { get; set; }
        public ObservableCollection<DayViewModel> WeekDays { get; set; } = new();
        public ObservableCollection<StudentSelection> AllStudentsSelection { get; set; } = new();

        // Źródło danych dla ComboBoxa ze statusami w edycji lekcji
        public IEnumerable<LessonStatus> AllStatuses => Enum.GetValues(typeof(LessonStatus)).Cast<LessonStatus>();

        public ObservableCollection<Student> GroupMembers
        {
            get => _groupMembers;
            set { _groupMembers = value; OnPropertyChanged(); }
        }

        public ObservableCollection<GroupSchedule> GroupSchedules
        {
            get => _groupSchedules;
            set { _groupSchedules = value; OnPropertyChanged(); }
        }
        #endregion

        #region Wybrane Obiekty (Selected)
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
                // Odpinamy zdarzenie od starej lekcji
                if (_selectedLesson != null) _selectedLesson.PropertyChanged -= OnModelPropertyChanged;

                _selectedLesson = value;

                // Podpinamy zdarzenie pod nową lekcję, by zmiany statusu/grupy wyzwalały IsDirty
                if (_selectedLesson != null) _selectedLesson.PropertyChanged += OnModelPropertyChanged;

                OnPropertyChanged();
                // Ważne: przy zmianie wyboru lekcji na inną, resetujemy flagę zmian
                IsDirty = false;
            }
        }

        public bool IsEditingMembers
        {
            get => _isEditingMembers;
            set { _isEditingMembers = value; OnPropertyChanged(); }
        }

        public string CurrentWeekTitle => $"{_currentWeekStart:dd.MM} - {_currentWeekStart.AddDays(6):dd.MM.yyyy}";
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
        public ICommand SelectLessonCommand { get; }
        public ICommand DeleteLessonCommand { get; }
        public ICommand SaveLessonCommand { get; }
        public ICommand AddManualLessonCommand { get; }
        #endregion

        public MainViewModel(IStudentService studentService, IGroupService groupService, ILessonService lessonService)
        {
            _studentService = studentService;
            _groupService = groupService;
            _lessonService = lessonService;

            Students = new ObservableCollection<Student>(_studentService.GetAllStudents());
            Groups = new ObservableCollection<Group>(_groupService.GetAllGroups());
            _groupMembers = new ObservableCollection<Student>();
            _groupSchedules = new ObservableCollection<GroupSchedule>();

            #region Inicjalizacja Komend
            AddStudentCommand = new RelayCommand(o => AddStudent(), o => !IsDirty);
            SaveStudentCommand = new RelayCommand(o => SaveStudent(), o => IsDirty);
            DeleteStudentCommand = new RelayCommand(o => DeleteStudent(), o => SelectedStudent != null && !IsDirty);
            CancelStudentCommand = new RelayCommand(o => CancelStudentChanges(), o => IsDirty && SelectedStudent != null);

            AddGroupCommand = new RelayCommand(o => AddGroup(), o => !IsDirty);
            SaveGroupCommand = new RelayCommand(o => SaveGroup(), o => IsDirty);
            DeleteGroupCommand = new RelayCommand(o => DeleteGroup(), o => SelectedGroup != null && !IsDirty);
            CancelGroupCommand = new RelayCommand(o => CancelGroupChanges(), o => IsDirty && SelectedGroup != null);

            StartEditMembersCommand = new RelayCommand(o => StartEditMembers(), o => SelectedGroup != null);
            SaveMembersCommand = new RelayCommand(o => SaveMembers());
            CancelEditMembersCommand = new RelayCommand(o => { IsEditingMembers = false; });

            AddScheduleCommand = new RelayCommand(o => AddSchedule(), o => SelectedGroup != null);
            DeleteScheduleCommand = new RelayCommand(o => DeleteSchedule(o as GroupSchedule), o => o is GroupSchedule);

            NextWeekCommand = new RelayCommand(o => ChangeWeek(7));
            PreviousWeekCommand = new RelayCommand(o => ChangeWeek(-7));

            SelectLessonCommand = new RelayCommand(o => {
                if (o is Lesson lesson) SelectedLesson = lesson;
                else SelectedLesson = null;
            });

            DeleteLessonCommand = new RelayCommand(async o => await DeleteLessonAsync(), o => SelectedLesson != null);
            SaveLessonCommand = new RelayCommand(async o => await SaveLessonAsync(), o => IsDirty && SelectedLesson != null);
            AddManualLessonCommand = new RelayCommand(o => AddManualLesson());
            #endregion

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

        private void RefreshList()
        {
            CollectionViewSource.GetDefaultView(Students)?.Refresh();
            CollectionViewSource.GetDefaultView(Groups)?.Refresh();
        }

        private void ApplyFilter()
        {
            var filtered = _allStudentsFullList
                .Where(s => string.IsNullOrWhiteSpace(SearchText) ||
                            s.Student.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            AllStudentsSelection.Clear();
            foreach (var item in filtered) AllStudentsSelection.Add(item);
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

        private void SaveGroup()
        {
            if (SelectedGroup == null) return;

            if (SelectedGroup.Id == 0) _groupService.AddGroup(SelectedGroup);
            _groupService.SaveChanges();

            SyncLessonsWithSchedule(SelectedGroup);

            IsDirty = false;
            RefreshList();
            _ = LoadWeekDataAsync();
        }

        private void SyncLessonsWithSchedule(Group group)
        {
            int weeksToGenerate = 10;
            DateTime startDate = DateTime.Today;

            foreach (var schedule in group.Schedules)
            {
                var lessonsToSync = _lessonService.GetFutureLessonsBySchedule(schedule.Id).ToList();

                foreach (var lesson in lessonsToSync)
                {
                    int daysOffset = (int)schedule.DayOfWeek - (int)lesson.StartTime.DayOfWeek;
                    DateTime newDate = lesson.StartTime.AddDays(daysOffset);

                    lesson.StartTime = newDate.Date.Add(schedule.StartTime);
                    lesson.EndTime = newDate.Date.Add(schedule.EndTime);

                    _lessonService.UpdateLessonAsync(lesson).Wait();
                }

                for (int i = 0; i < weeksToGenerate; i++)
                {
                    DateTime lessonDate = startDate.AddDays(i * 7);
                    int daysUntilNextDay = ((int)schedule.DayOfWeek - (int)lessonDate.DayOfWeek + 7) % 7;
                    lessonDate = lessonDate.AddDays(daysUntilNextDay);

                    DateTime finalStart = lessonDate.Date.Add(schedule.StartTime);
                    bool exists = _lessonService.LessonExists(group.Id, schedule.Id, finalStart);

                    if (!exists)
                    {
                        var newLesson = new Lesson
                        {
                            GroupId = group.Id,
                            GroupScheduleId = schedule.Id,
                            StartTime = finalStart,
                            EndTime = lessonDate.Date.Add(schedule.EndTime),
                            Status = LessonStatus.Scheduled,
                            Note = "Lekcja generowana automatycznie"
                        };
                        _lessonService.AddLessonAsync(newLesson).Wait();
                    }
                }
            }
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
            }
            _isInternalUpdate = false;
            IsEditingMembers = false;
            IsDirty = false;
            RefreshList();
        }

        private void DeleteGroup()
        {
            if (SelectedGroup == null) return;
            if (MessageBox.Show($"Usunąć grupę {SelectedGroup.Name}?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedGroup.Id > 0)
                {
                    _groupService.DeleteGroup(SelectedGroup);
                    _groupService.SaveChanges();
                }
                Groups.Remove(SelectedGroup);
                SelectedGroup = null;
                IsDirty = false;
            }
        }

        private void LoadGroupMembers()
        {
            if (SelectedGroup == null || SelectedGroup.Id == 0)
            {
                GroupMembers = new ObservableCollection<Student>();
                return;
            }
            var members = SelectedGroup.Enrollments.Select(e => e.Student).ToList();
            GroupMembers = new ObservableCollection<Student>(members);
        }

        private void LoadGroupSchedules()
        {
            if (GroupSchedules != null)
            {
                foreach (var s in GroupSchedules)
                    s.PropertyChanged -= OnSchedulePropertyChanged;
            }

            if (SelectedGroup == null)
            {
                GroupSchedules = new ObservableCollection<GroupSchedule>();
                return;
            }

            GroupSchedules = new ObservableCollection<GroupSchedule>(SelectedGroup.Schedules);

            foreach (var schedule in GroupSchedules)
            {
                schedule.PropertyChanged += OnSchedulePropertyChanged;
            }
        }

        private void AddSchedule()
        {
            if (SelectedGroup == null) return;

            var newSchedule = new GroupSchedule
            {
                GroupId = SelectedGroup.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeSpan(16, 0, 0),
                EndTime = new TimeSpan(17, 30, 0)
            };

            newSchedule.PropertyChanged += OnSchedulePropertyChanged;

            SelectedGroup.Schedules.Add(newSchedule);
            GroupSchedules.Add(newSchedule);
            IsDirty = true;
        }

        private void DeleteSchedule(GroupSchedule? schedule)
        {
            if (SelectedGroup == null || schedule == null) return;

            schedule.PropertyChanged -= OnSchedulePropertyChanged;

            SelectedGroup.Schedules.Remove(schedule);
            GroupSchedules.Remove(schedule);
            IsDirty = true;
        }

        private void StartEditMembers()
        {
            if (SelectedGroup == null || SelectedGroup.Id == 0) return;
            _allStudentsFullList.Clear();
            var currentMemberIds = GroupMembers.Select(s => s.Id).ToList();
            foreach (var student in Students)
                _allStudentsFullList.Add(new StudentSelection(student, currentMemberIds.Contains(student.Id)));

            SearchText = "";
            ApplyFilter();
            IsEditingMembers = true;
            IsDirty = true;
        }

        private void SaveMembers()
        {
            if (SelectedGroup == null) return;
            var selectedStudents = AllStudentsSelection.Where(s => s.IsSelected).Select(s => s.Student).ToList();
            _groupService.UpdateGroupMembers(SelectedGroup, selectedStudents);
            LoadGroupMembers();
            IsEditingMembers = false;
            IsDirty = false;
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

            Application.Current.Dispatcher.Invoke(() =>
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

        private async void ChangeWeek(int days)
        {
            SelectedLesson = null;
            _currentWeekStart = _currentWeekStart.AddDays(days);
            OnPropertyChanged(nameof(CurrentWeekTitle));
            await LoadWeekDataAsync();
        }

        private void AddManualLesson()
        {
            var newLesson = new Lesson
            {
                // Domyślna data: dzisiaj o 16:00
                StartTime = DateTime.Today.AddHours(16),
                EndTime = DateTime.Today.AddHours(17).AddMinutes(30),
                Status = LessonStatus.Scheduled,
                Note = "Nowa lekcja",
                // Przypisanie grupy, jeśli użytkownik jakąś aktualnie przegląda
                GroupId = SelectedGroup?.Id ?? (Groups.FirstOrDefault()?.Id ?? 0)
            };

            SelectedLesson = newLesson;
            IsDirty = true; // Pozwala od razu kliknąć "Zapisz"
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
        }

        private async Task DeleteLessonAsync()
        {
            if (SelectedLesson == null) return;
            if (MessageBox.Show("Usunąć tę lekcję z kalendarza?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedLesson.Id > 0)
                    await _lessonService.DeleteLessonAsync(SelectedLesson.Id);

                SelectedLesson = null;
                await LoadWeekDataAsync();
            }
        }
        #endregion
    }
}