using Learning_Management_System.ViewModels;
using System.Windows;

namespace Learning_Management_System
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }


    }
}