using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Windows.Data;

namespace Learning_Management_System.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AppDbContext _db;

        private Student? _selectedStudent;
        private Group? _selectedGroup;
        private ObservableCollection<Student> _groupMembers;
        private bool _isEditingMembers;
        private bool _isDirty;

        // NOWOŚĆ: Flaga blokująca wykrywanie zmian podczas anulowania
        private bool _isInternalUpdate = false;

        private string _searchText = string.Empty;
        private List<StudentSelection> _allStudentsFullList = new();

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

        private ObservableCollection<Student> _students;
        public ObservableCollection<Student> Students
        {
            get => _students;
            set { _students = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Group> _groups;
        public ObservableCollection<Group> Groups
        {
            get => _groups;
            set { _groups = value; OnPropertyChanged(); }
        }

        public ObservableCollection<StudentSelection> AllStudentsSelection { get; set; } = new();

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent == value) return;

                if (_selectedStudent != null)
                    _selectedStudent.PropertyChanged -= OnModelPropertyChanged;

                _selectedStudent = value;

                if (_selectedStudent != null)
                    _selectedStudent.PropertyChanged += OnModelPropertyChanged;

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

                if (_selectedGroup != null)
                    _selectedGroup.PropertyChanged -= OnModelPropertyChanged;

                _selectedGroup = value;

                if (_selectedGroup != null)
                    _selectedGroup.PropertyChanged += OnModelPropertyChanged;

                OnPropertyChanged();
                IsEditingMembers = false;
                IsDirty = false;
                LoadGroupMembers();
            }
        }

        private void OnModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // JEŚLI aktualnie przywracamy dane (Anuluj), to ignorujemy zmiany
            if (_isInternalUpdate) return;

            // W każdym innym przypadku włączamy przycisk Zapisz
            IsDirty = true;
        }

        public ObservableCollection<Student> GroupMembers
        {
            get => _groupMembers;
            set { _groupMembers = value; OnPropertyChanged(); }
        }

        public bool IsEditingMembers
        {
            get => _isEditingMembers;
            set { _isEditingMembers = value; OnPropertyChanged(); }
        }

        // Komendy
        public ICommand AddStudentCommand { get; }
        public ICommand SaveStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand CancelStudentCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand SaveGroupCommand { get; }
        public ICommand StartEditMembersCommand { get; }
        public ICommand SaveMembersCommand { get; }
        public ICommand CancelEditMembersCommand { get; }

        public MainViewModel()
        {
            _db = new AppDbContext();
            _students = new ObservableCollection<Student>(_db.Students.OrderBy(s => s.LastName).ToList());
            _groups = new ObservableCollection<Group>(_db.Groups.ToList());
            _groupMembers = new ObservableCollection<Student>();

            AddStudentCommand = new RelayCommand(o => AddStudent(), o => !IsDirty);
            SaveStudentCommand = new RelayCommand(o => SaveStudent(), o => IsDirty);
            DeleteStudentCommand = new RelayCommand(o => DeleteStudent(), o => SelectedStudent != null && !IsDirty);
            CancelStudentCommand = new RelayCommand(o => CancelChanges(), o => IsDirty);

            AddGroupCommand = new RelayCommand(o => AddGroup(), o => !IsDirty);
            SaveGroupCommand = new RelayCommand(o => SaveGroup(), o => IsDirty);

            StartEditMembersCommand = new RelayCommand(o => StartEditMembers(), o => SelectedGroup != null && !IsDirty);
            SaveMembersCommand = new RelayCommand(o => SaveMembers());
            CancelEditMembersCommand = new RelayCommand(o => { IsEditingMembers = false; IsDirty = false; });
        }

        private void AddStudent()
        {
            var newStudent = new Student { FirstName = "Nowy", LastName = "Uczeń" };
            Students.Add(newStudent);
            SelectedStudent = newStudent;
            IsDirty = true;
        }

        private void SaveStudent()
        {
            if (SelectedStudent == null) return;
            if (SelectedStudent.Id == 0) _db.Students.Add(SelectedStudent);

            _db.SaveChanges();
            IsDirty = false;
            RefreshList();
        }

        // --- POPRAWIONA METODA ANULOWANIA ---
        private void CancelChanges()
        {
            if (SelectedStudent == null) return;

            var entry = _db.Entry(SelectedStudent);

            // Scenariusz 1: Anulujemy dodawanie zupełnie nowego ucznia
            if (entry.State == EntityState.Added)
            {
                Students.Remove(SelectedStudent);
                SelectedStudent = null; // Czyścimy formularz
            }
            // Scenariusz 2: Anulujemy edycję istniejącego ucznia
            else
            {
                // 1. Blokujemy flagę, żeby przywracanie danych nie włączyło znowu IsDirty
                _isInternalUpdate = true;

                // 2. Twardy reset danych w obiekcie do tego co jest w bazie
                entry.Reload();

                // 3. TRIK: Musimy zmusić widok do odświeżenia TextBoxów.
                // Robimy to przez chwilowe "mignięcie" selekcją.
                var tempStudent = SelectedStudent;
                SelectedStudent = null;       // Odpięcie
                SelectedStudent = tempStudent; // Ponowne przypięcie -> widok pobiera stare dane

                // 4. Odblokowujemy flagę
                _isInternalUpdate = false;
            }

            IsDirty = false;
            RefreshList(); // Odświeżamy listę po lewej
        }

        private void RefreshList()
        {
            CollectionViewSource.GetDefaultView(Students).Refresh();
            CollectionViewSource.GetDefaultView(Groups).Refresh();
        }

        private void DeleteStudent()
        {
            if (SelectedStudent == null) return;
            if (MessageBox.Show($"Usunąć ucznia {SelectedStudent.FullName}?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedStudent.Id > 0)
                {
                    _db.Students.Remove(SelectedStudent);
                    _db.SaveChanges();
                }
                Students.Remove(SelectedStudent);
                SelectedStudent = null;
                IsDirty = false;
            }
        }

        private void AddGroup()
        {
            var newGroup = new Group { Name = "Nowa Grupa", BaseRate = 0 };
            Groups.Add(newGroup);
            SelectedGroup = newGroup;
            IsDirty = true;
        }

        private void SaveGroup()
        {
            if (SelectedGroup == null) return;
            if (SelectedGroup.Id == 0) _db.Groups.Add(SelectedGroup);
            _db.SaveChanges();
            IsDirty = false;
            RefreshList();
        }

        private void LoadGroupMembers()
        {
            if (SelectedGroup == null || SelectedGroup.Id == 0)
            {
                GroupMembers = new ObservableCollection<Student>();
                return;
            }
            var members = _db.Enrollments
                .Where(e => e.GroupId == SelectedGroup.Id)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToList();
            GroupMembers = new ObservableCollection<Student>(members);
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
            var toRemove = _db.Enrollments.Where(e => e.GroupId == SelectedGroup.Id);
            _db.Enrollments.RemoveRange(toRemove);

            foreach (var selection in AllStudentsSelection.Where(s => s.IsSelected))
                _db.Enrollments.Add(new Enrollment { GroupId = SelectedGroup.Id, StudentId = selection.Student.Id });

            _db.SaveChanges();
            LoadGroupMembers();
            IsEditingMembers = false;
            IsDirty = false;
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
    }
}