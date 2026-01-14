using Learning_Management_System.Data;
using Learning_Management_System.Services;
using Learning_Management_System.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace Learning_Management_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();

            // Initialize database and apply migrations
            dbContext.Database.Migrate();

            var studentService = new StudentService(dbContext);
            var groupService = new GroupService(dbContext);
            var lessonService = new LessonService(dbContext);
            var paymentService = new PaymentService(dbContext);
            var attendanceService = new AttendanceService(dbContext);
            var reportService = new ReportService(dbContext, paymentService);
            var exportService = new ExportService();
            var backupService = new BackupService(dbContext);

            // Przekaż lessonService jako trzeci parametr
            DataContext = new MainViewModel(studentService, groupService, lessonService, paymentService, attendanceService, reportService, exportService, backupService, dbContext);
        }

        private void LessonCard_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is MaterialDesignThemes.Wpf.Card card && card.Tag is Models.Lesson lesson)
            {
                if (DataContext is ViewModels.MainViewModel mainViewModel)
                {
                    mainViewModel.LessonBrowserViewModel.SelectLessonCommand.Execute(lesson);
                }
            }
        }

        private async void ArchiveTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TabControl tabControl && 
                tabControl.SelectedItem is System.Windows.Controls.TabItem selectedTab &&
                selectedTab.Tag is string archiveType &&
                DataContext is ViewModels.MainViewModel mainViewModel)
            {
                mainViewModel.ArchiveViewModel.SelectedArchiveType = archiveType;
                await mainViewModel.ArchiveViewModel.LoadArchivedDataAsync();
            }
        }

        private async void ArchiveTabItem_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel mainViewModel)
            {
                await mainViewModel.ArchiveViewModel.LoadArchivedDataAsync();
            }
        }
    }
}