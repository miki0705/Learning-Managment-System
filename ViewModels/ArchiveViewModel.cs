using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Learning_Management_System.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Learning_Management_System.ViewModels
{
    public class ArchiveViewModel : ViewModelBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IStudentService _studentService;
        private readonly IGroupService _groupService;
        private readonly ILessonService _lessonService;
        private readonly IPaymentService _paymentService;
        private readonly IAttendanceService _attendanceService;

        private string _selectedArchiveType = "Students";

        public ArchiveViewModel(
            AppDbContext dbContext,
            IStudentService studentService,
            IGroupService groupService,
            ILessonService lessonService,
            IPaymentService paymentService,
            IAttendanceService attendanceService)
        {
            _dbContext = dbContext;
            _studentService = studentService;
            _groupService = groupService;
            _lessonService = lessonService;
            _paymentService = paymentService;
            _attendanceService = attendanceService;

            ArchivedStudents = new ObservableCollection<Student>();
            ArchivedGroups = new ObservableCollection<Group>();
            ArchivedLessons = new ObservableCollection<Lesson>();
            ArchivedPayments = new ObservableCollection<Payment>();
            ArchivedAttendances = new ObservableCollection<Attendance>();

            RestoreStudentCommand = new AsyncRelayCommand(
                async o => 
                {
                    // #region agent log
                    try 
                    { 
                        var logData = new 
                        { 
                            sessionId = "debug-session", 
                            runId = "run1", 
                            hypothesisId = "A,C", 
                            location = "ArchiveViewModel.cs:RestoreStudentCommand", 
                            message = "Command lambda entry", 
                            data = new 
                            { 
                                parameterType = o?.GetType().Name,
                                parameterValue = o?.ToString(),
                                isNull = o == null
                            }, 
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                        }; 
                        var json = JsonSerializer.Serialize(logData);
                        File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", json + Environment.NewLine);
                        Debug.WriteLine($"[LOG] Command lambda: {json}");
                    } 
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[LOG ERROR] {ex.Message}");
                    }
                    // #endregion
                    
                    if (o != null)
                    {
                        await RestoreStudentAsync(o as Student ?? (Student)o);
                    }
                }, 
                o => 
                {
                    // #region agent log
                    try 
                    { 
                        var logData = new 
                        { 
                            sessionId = "debug-session", 
                            runId = "run1", 
                            hypothesisId = "C", 
                            location = "ArchiveViewModel.cs:RestoreStudentCommand.CanExecute", 
                            message = "CanExecute check", 
                            data = new 
                            { 
                                parameterType = o?.GetType().Name,
                                isNull = o == null,
                                result = o != null
                            }, 
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                        }; 
                        var json = JsonSerializer.Serialize(logData);
                        File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", json + Environment.NewLine);
                    } 
                    catch { }
                    // #endregion
                    
                    return o != null;
                });
            RestoreGroupCommand = new AsyncRelayCommand(
                async o => 
                {
                    if (o != null)
                    {
                        await RestoreGroupAsync(o as Group ?? (Group)o);
                    }
                }, 
                o => o != null);
            RestoreLessonCommand = new AsyncRelayCommand(
                async o => 
                {
                    if (o != null)
                    {
                        await RestoreLessonAsync(o as Lesson ?? (Lesson)o);
                    }
                }, 
                o => o != null);
            RestorePaymentCommand = new AsyncRelayCommand(
                async o => 
                {
                    if (o != null)
                    {
                        await RestorePaymentAsync(o as Payment ?? (Payment)o);
                    }
                }, 
                o => o != null);
            RestoreAttendanceCommand = new AsyncRelayCommand(
                async o => 
                {
                    if (o != null)
                    {
                        await RestoreAttendanceAsync(o as Attendance ?? (Attendance)o);
                    }
                }, 
                o => o != null);

            // Initialize with Students tab
            SelectedArchiveType = "Students";
        }

        public ObservableCollection<Student> ArchivedStudents { get; set; }
        public ObservableCollection<Group> ArchivedGroups { get; set; }
        public ObservableCollection<Lesson> ArchivedLessons { get; set; }
        public ObservableCollection<Payment> ArchivedPayments { get; set; }
        public ObservableCollection<Attendance> ArchivedAttendances { get; set; }

        public string SelectedArchiveType
        {
            get => _selectedArchiveType;
            set
            {
                if (_selectedArchiveType != value)
                {
                    _selectedArchiveType = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RestoreStudentCommand { get; }
        public ICommand RestoreGroupCommand { get; }
        public ICommand RestoreLessonCommand { get; }
        public ICommand RestorePaymentCommand { get; }
        public ICommand RestoreAttendanceCommand { get; }

        // Test method to verify command binding
        public void TestCommandBinding()
        {
            // #region agent log
            try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "ArchiveViewModel.cs:55", message = "TestCommandBinding called", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
            // #endregion
        }

        public async Task LoadArchivedDataAsync()
        {
            await LoadArchivedStudentsAsync();
            await LoadArchivedGroupsAsync();
            await LoadArchivedLessonsAsync();
            await LoadArchivedPaymentsAsync();
            await LoadArchivedAttendancesAsync();
        }

        private async Task LoadArchivedStudentsAsync()
        {
            var students = await _dbContext.Students
                .IgnoreQueryFilters()
                .Where(s => s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            ArchivedStudents.Clear();
            foreach (var student in students)
            {
                ArchivedStudents.Add(student);
            }
        }

        private async Task LoadArchivedGroupsAsync()
        {
            var groups = await _dbContext.Groups
                .IgnoreQueryFilters()
                .Include(g => g.Schedules)
                .Include(g => g.Enrollments)
                .Where(g => g.IsDeleted)
                .OrderBy(g => g.Name)
                .ToListAsync();

            ArchivedGroups.Clear();
            foreach (var group in groups)
            {
                ArchivedGroups.Add(group);
            }
        }

        private async Task LoadArchivedLessonsAsync()
        {
            var lessons = await _dbContext.Lessons
                .IgnoreQueryFilters()
                .Include(l => l.Group)
                .Include(l => l.Attendances)
                .Where(l => l.IsDeleted)
                .OrderByDescending(l => l.StartTime)
                .ToListAsync();

            ArchivedLessons.Clear();
            foreach (var lesson in lessons)
            {
                ArchivedLessons.Add(lesson);
            }
        }

        private async Task LoadArchivedPaymentsAsync()
        {
            var payments = await _dbContext.Payments
                .IgnoreQueryFilters()
                .Include(p => p.Student)
                .Where(p => p.IsDeleted)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            ArchivedPayments.Clear();
            foreach (var payment in payments)
            {
                ArchivedPayments.Add(payment);
            }
        }

        private async Task LoadArchivedAttendancesAsync()
        {
            var attendances = await _dbContext.Attendances
                .IgnoreQueryFilters()
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => a.IsDeleted)
                .OrderByDescending(a => a.Lesson.StartTime)
                .ToListAsync();

            ArchivedAttendances.Clear();
            foreach (var attendance in attendances)
            {
                ArchivedAttendances.Add(attendance);
            }
        }

        private async Task RestoreStudentAsync(Student? student)
        {
            System.Windows.MessageBox.Show("METODA WYWOŁANA!");

            // #region agent log
            try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A,B,C", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "RestoreStudentAsync entry", data = new { studentId = student?.Id ?? -1, studentName = student?.FullName, isNull = student == null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
            // #endregion
            
            Debug.WriteLine($"RestoreStudentAsync called with student: {student?.Id ?? -1}");
            
            if (student == null)
            {
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Student is null", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                Debug.WriteLine("Student is null, returning");
                return;
            }

            try
            {
                // CRITICAL: Re-fetch the entity using IgnoreQueryFilters() to ensure it's tracked by the context
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Before re-fetching entity", data = new { studentId = student.Id }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                
                var trackedStudent = await _dbContext.Students
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == student.Id);
                
                if (trackedStudent == null)
                {
                    // #region agent log
                    try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Entity not found in database", data = new { studentId = student.Id }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                    // #endregion
                    MessageBox.Show($"Nie znaleziono ucznia o ID {student.Id} w bazie danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Entity re-fetched, before setting IsDeleted", data = new { studentId = trackedStudent.Id, currentIsDeleted = trackedStudent.IsDeleted, entryState = _dbContext.Entry(trackedStudent).State.ToString() }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                
                Debug.WriteLine($"Restoring student ID: {trackedStudent.Id}, Name: {trackedStudent.FullName}");
                
                trackedStudent.IsDeleted = false;
                trackedStudent.IsActive = true; // Also restore IsActive flag
                _dbContext.Entry(trackedStudent).State = EntityState.Modified;
                
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Before SaveChangesAsync", data = new { studentId = trackedStudent.Id, entryState = _dbContext.Entry(trackedStudent).State.ToString(), isDeleted = trackedStudent.IsDeleted, isActive = trackedStudent.IsActive }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                
                int changes = 0;
                try
                {
                    changes = await _dbContext.SaveChangesAsync();
                    
                    // #region agent log
                    try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "After SaveChangesAsync", data = new { changesSaved = changes, studentId = trackedStudent.Id, isDeletedAfter = trackedStudent.IsDeleted }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                    // #endregion
                    
                    Debug.WriteLine($"SaveChangesAsync completed. Changes saved: {changes}");
                }
                catch (Exception saveEx)
                {
                    // #region agent log
                    try 
                    { 
                        var logData = new 
                        { 
                            sessionId = "debug-session", 
                            runId = "run1", 
                            hypothesisId = "A", 
                            location = "ArchiveViewModel.cs:RestoreStudentAsync", 
                            message = "Exception during SaveChangesAsync", 
                            data = new 
                            {
                                exceptionType = saveEx.GetType().Name,
                                message = saveEx.Message,
                                innerException = saveEx.InnerException?.Message,
                                stackTrace = saveEx.StackTrace,
                                innerStackTrace = saveEx.InnerException?.StackTrace
                            }, 
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() 
                        }; 
                        File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); 
                    } 
                    catch { }
                    // #endregion
                    
                    var errorMessage = $"Błąd podczas zapisywania zmian: {saveEx.Message}";
                    if (saveEx.InnerException != null)
                    {
                        errorMessage += $"\n\nSzczegóły: {saveEx.InnerException.Message}";
                    }
                    errorMessage += $"\n\nStack trace: {saveEx.StackTrace}";
                    
                    Debug.WriteLine($"ERROR in SaveChangesAsync: {errorMessage}");
                    MessageBox.Show(errorMessage, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Remove from archive collection (use the original student parameter for removal)
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "Before removing from collection", data = new { collectionCount = ArchivedStudents.Count, studentId = student.Id }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                
                ArchivedStudents.Remove(student);
                
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "ArchiveViewModel.cs:RestoreStudentAsync", message = "After removing from collection", data = new { collectionCount = ArchivedStudents.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + Environment.NewLine); } catch { }
                // #endregion
                
                Debug.WriteLine("Student removed from ArchivedStudents collection");

                // Reload archive data to refresh the list
                await LoadArchivedDataAsync();
                Debug.WriteLine("Archive data reloaded");

                // Refresh main collections
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "ArchiveViewModel.cs:220", message = "Before MainViewModel refresh", data = new { hasMainWindow = Application.Current?.MainWindow != null, dataContextType = Application.Current?.MainWindow?.DataContext?.GetType().Name }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                // #endregion
                
                if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    // #region agent log
                    try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "ArchiveViewModel.cs:224", message = "Calling RefreshStudents", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                    // #endregion
                    
                    Debug.WriteLine("Calling RefreshStudents on MainViewModel");
                    mainViewModel.RefreshStudents();
                    
                    // #region agent log
                    try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "ArchiveViewModel.cs:229", message = "After RefreshStudents", data = new { studentsCount = mainViewModel.Students.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                    // #endregion
                    
                    Debug.WriteLine("RefreshStudents completed");
                }
                else
                {
                    // #region agent log
                    try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "ArchiveViewModel.cs:236", message = "MainViewModel not found", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                    // #endregion
                    Debug.WriteLine("WARNING: Could not find MainViewModel in DataContext");
                }

                MessageBox.Show("Uczeń został przywrócony.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "ArchiveViewModel.cs:243", message = "RestoreStudentAsync success exit", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                // #endregion
            }
            catch (System.Exception ex)
            {
                // #region agent log
                try { var logData = new { sessionId = "debug-session", runId = "run1", hypothesisId = "A,E", location = "ArchiveViewModel.cs:248", message = "Exception in RestoreStudentAsync", data = new { exceptionType = ex.GetType().Name, message = ex.Message, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }; File.AppendAllText(@"d:\Work\PersonalProjects\Learning-Managment-System\.cursor\debug.log", JsonSerializer.Serialize(logData) + "\n"); } catch { }
                // #endregion
                
                Debug.WriteLine($"ERROR in RestoreStudentAsync: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Błąd podczas przywracania ucznia: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RestoreGroupAsync(Group? group)
        {
            if (group == null) return;

            try
            {
                // CRITICAL: Re-fetch the entity using IgnoreQueryFilters() to ensure it's tracked by the context
                var trackedGroup = await _dbContext.Groups
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(g => g.Id == group.Id);
                
                if (trackedGroup == null)
                {
                    MessageBox.Show($"Nie znaleziono grupy o ID {group.Id} w bazie danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool cascadeRestored = false;

                // Restore GroupSchedules
                var schedules = await _dbContext.GroupSchedules
                    .IgnoreQueryFilters()
                    .Where(gs => gs.GroupId == trackedGroup.Id && gs.IsDeleted)
                    .ToListAsync();

                foreach (var schedule in schedules)
                {
                    schedule.IsDeleted = false;
                    _dbContext.Entry(schedule).State = EntityState.Modified;
                    cascadeRestored = true;
                }

                // Restore Enrollments
                var enrollments = await _dbContext.Enrollments
                    .IgnoreQueryFilters()
                    .Where(e => e.GroupId == trackedGroup.Id && e.IsDeleted)
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    enrollment.IsDeleted = false;
                    _dbContext.Entry(enrollment).State = EntityState.Modified;
                    cascadeRestored = true;
                }

                // Restore the Group
                trackedGroup.IsDeleted = false;
                _dbContext.Entry(trackedGroup).State = EntityState.Modified;
                
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception saveEx)
                {
                    var errorMessage = $"Błąd podczas zapisywania zmian: {saveEx.Message}";
                    if (saveEx.InnerException != null)
                    {
                        errorMessage += $"\n\nSzczegóły: {saveEx.InnerException.Message}";
                    }
                    errorMessage += $"\n\nStack trace: {saveEx.StackTrace}";
                    MessageBox.Show(errorMessage, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ArchivedGroups.Remove(group);
                await LoadArchivedDataAsync();

                // Refresh main collections
                if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.RefreshGroups();
                }

                if (cascadeRestored)
                {
                    MessageBox.Show("Grupa została przywrócona. Powiązane rekordy (harmonogramy i zapisy) zostały również przywrócone, aby zapewnić spójność danych.", 
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Grupa została przywrócona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Błąd podczas przywracania grupy: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RestoreLessonAsync(Lesson? lesson)
        {
            if (lesson == null) return;

            try
            {
                bool cascadeRestored = false;

            // Restore Group if deleted
            var group = await _dbContext.Groups
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(g => g.Id == lesson.GroupId && g.IsDeleted);

            if (group != null)
            {
                group.IsDeleted = false;
                _dbContext.Entry(group).State = EntityState.Modified;
                cascadeRestored = true;
            }

            // Restore Students from Attendances if deleted
            var studentIds = await _dbContext.Attendances
                .IgnoreQueryFilters()
                .Where(a => a.LessonId == lesson.Id)
                .Select(a => a.StudentId)
                .Distinct()
                .ToListAsync();

            var deletedStudents = await _dbContext.Students
                .IgnoreQueryFilters()
                .Where(s => studentIds.Contains(s.Id) && s.IsDeleted)
                .ToListAsync();

            foreach (var student in deletedStudents)
            {
                student.IsDeleted = false;
                _dbContext.Entry(student).State = EntityState.Modified;
                cascadeRestored = true;
            }

            // CRITICAL: Re-fetch the entity using IgnoreQueryFilters() to ensure it's tracked by the context
            var trackedLesson = await _dbContext.Lessons
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.Id == lesson.Id);
            
            if (trackedLesson == null)
            {
                MessageBox.Show($"Nie znaleziono lekcji o ID {lesson.Id} w bazie danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Restore the Lesson
            trackedLesson.IsDeleted = false;
            _dbContext.Entry(trackedLesson).State = EntityState.Modified;
            
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception saveEx)
            {
                var errorMessage = $"Błąd podczas zapisywania zmian: {saveEx.Message}";
                if (saveEx.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {saveEx.InnerException.Message}";
                }
                errorMessage += $"\n\nStack trace: {saveEx.StackTrace}";
                MessageBox.Show(errorMessage, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

                ArchivedLessons.Remove(lesson);
                await LoadArchivedDataAsync();

                // Refresh main collections
                if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.RefreshGroups();
                    mainViewModel.RefreshStudents();
                }

                if (cascadeRestored)
                {
                    MessageBox.Show("Lekcja została przywrócona. Powiązane rekordy (grupy i uczniowie) zostały również przywrócone, aby zapewnić spójność danych.", 
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Lekcja została przywrócona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Błąd podczas przywracania lekcji: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RestorePaymentAsync(Payment? payment)
        {
            if (payment == null) return;

            try
            {
                bool cascadeRestored = false;

            // Restore Student if deleted
            var student = await _dbContext.Students
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == payment.StudentId && s.IsDeleted);

            if (student != null)
            {
                student.IsDeleted = false;
                _dbContext.Entry(student).State = EntityState.Modified;
                cascadeRestored = true;
            }

            // CRITICAL: Re-fetch the entity using IgnoreQueryFilters() to ensure it's tracked by the context
            var trackedPayment = await _dbContext.Payments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == payment.Id);
            
            if (trackedPayment == null)
            {
                MessageBox.Show($"Nie znaleziono płatności o ID {payment.Id} w bazie danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Restore the Payment
            trackedPayment.IsDeleted = false;
            _dbContext.Entry(trackedPayment).State = EntityState.Modified;
            
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception saveEx)
            {
                var errorMessage = $"Błąd podczas zapisywania zmian: {saveEx.Message}";
                if (saveEx.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {saveEx.InnerException.Message}";
                }
                errorMessage += $"\n\nStack trace: {saveEx.StackTrace}";
                MessageBox.Show(errorMessage, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

                ArchivedPayments.Remove(payment);
                await LoadArchivedDataAsync();

                // Refresh main collections
                if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.RefreshStudents();
                }

                if (cascadeRestored)
                {
                    MessageBox.Show("Płatność została przywrócona. Powiązany uczeń został również przywrócony, aby zapewnić spójność danych.", 
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Płatność została przywrócona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Błąd podczas przywracania płatności: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RestoreAttendanceAsync(Attendance? attendance)
        {
            if (attendance == null) return;

            try
            {
                bool cascadeRestored = false;

                // Restore Student if deleted
                var student = await _dbContext.Students
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(s => s.Id == attendance.StudentId && s.IsDeleted);

                if (student != null)
                {
                    student.IsDeleted = false;
                    _dbContext.Entry(student).State = EntityState.Modified;
                    cascadeRestored = true;
                }

                // Restore Lesson if deleted
                var lesson = await _dbContext.Lessons
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Id == attendance.LessonId && l.IsDeleted);

                if (lesson != null)
                {
                    lesson.IsDeleted = false;
                    _dbContext.Entry(lesson).State = EntityState.Modified;
                    cascadeRestored = true;
                }

                // CRITICAL: Re-fetch the entity using IgnoreQueryFilters() to ensure it's tracked by the context
                var trackedAttendance = await _dbContext.Attendances
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a => a.Id == attendance.Id);
                
                if (trackedAttendance == null)
                {
                    MessageBox.Show($"Nie znaleziono frekwencji o ID {attendance.Id} w bazie danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Restore the Attendance
                trackedAttendance.IsDeleted = false;
                _dbContext.Entry(trackedAttendance).State = EntityState.Modified;
                
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception saveEx)
                {
                    var errorMessage = $"Błąd podczas zapisywania zmian: {saveEx.Message}";
                    if (saveEx.InnerException != null)
                    {
                        errorMessage += $"\n\nSzczegóły: {saveEx.InnerException.Message}";
                    }
                    errorMessage += $"\n\nStack trace: {saveEx.StackTrace}";
                    MessageBox.Show(errorMessage, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ArchivedAttendances.Remove(attendance);
                await LoadArchivedDataAsync();

                // Refresh main collections
                if (Application.Current?.MainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.RefreshStudents();
                }

                if (cascadeRestored)
                {
                    MessageBox.Show("Frekwencja została przywrócona. Powiązane rekordy (uczeń lub lekcja) zostały również przywrócone, aby zapewnić spójność danych.", 
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Frekwencja została przywrócona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Błąd podczas przywracania frekwencji: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nSzczegóły: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMessage, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
