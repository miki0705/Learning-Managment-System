using Learning_Management_System.Data;
using Learning_Management_System.Services;
using Learning_Management_System.ViewModels;
using System.Windows;

namespace Learning_Management_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var dbContext = new AppDbContext();

            var studentService = new StudentService(dbContext);
            var groupService = new GroupService(dbContext);
            var lessonService = new LessonService(dbContext); // DODAJ TO

            // Przekaż lessonService jako trzeci parametr
            DataContext = new MainViewModel(studentService, groupService, lessonService);
        }
    }
}