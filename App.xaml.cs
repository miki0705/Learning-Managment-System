using System.Configuration;
using System.Data;
using System.Windows;

namespace Learning_Management_System
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Handle unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Wystąpił błąd: {e.Exception.Message}\n\nSzczegóły: {e.Exception}", 
                "Błąd aplikacji", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Wystąpił krytyczny błąd: {ex.Message}\n\nSzczegóły: {ex}", 
                    "Krytyczny błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
