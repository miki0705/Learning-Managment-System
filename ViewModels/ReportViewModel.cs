using Learning_Management_System.Models;
using Learning_Management_System.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Learning_Management_System.ViewModels
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;
        private readonly IExportService _exportService;

        private string _selectedReportType = "Revenue";
        private DateTime? _reportStartDate = DateTime.Today.AddMonths(-1);
        private DateTime? _reportEndDate = DateTime.Today;
        private object? _currentReport;

        public ReportViewModel(IReportService reportService, IExportService exportService)
        {
            _reportService = reportService;
            _exportService = exportService;

            ReportTypes = new ObservableCollection<string> { "Revenue", "Attendance", "Wallet", "Lesson" };
            GenerateReportCommand = new RelayCommand(async o => await GenerateReportAsync());
            ExportToPdfCommand = new RelayCommand(async o => await ExportToPdfAsync(), o => CurrentReport != null);
            ExportToCsvCommand = new RelayCommand(async o => await ExportToCsvAsync(), o => CurrentReport != null);
        }

        public ObservableCollection<string> ReportTypes { get; set; }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (_selectedReportType == value) return;
                _selectedReportType = value;
                OnPropertyChanged();
            }
        }

        public DateTime? ReportStartDate
        {
            get => _reportStartDate;
            set
            {
                if (_reportStartDate == value) return;
                _reportStartDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? ReportEndDate
        {
            get => _reportEndDate;
            set
            {
                if (_reportEndDate == value) return;
                _reportEndDate = value;
                OnPropertyChanged();
            }
        }

        public object? CurrentReport
        {
            get => _currentReport;
            set
            {
                if (_currentReport == value) return;
                _currentReport = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportToPdfCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        private async Task GenerateReportAsync()
        {
            if (!ReportStartDate.HasValue || !ReportEndDate.HasValue)
            {
                MessageBox.Show("Wybierz zakres dat.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                switch (SelectedReportType)
                {
                    case "Revenue":
                        CurrentReport = await _reportService.GenerateRevenueReportAsync(ReportStartDate.Value, ReportEndDate.Value);
                        break;
                    case "Attendance":
                        CurrentReport = await _reportService.GenerateAttendanceReportAsync(ReportStartDate.Value, ReportEndDate.Value);
                        break;
                    case "Wallet":
                        CurrentReport = await _reportService.GenerateWalletReportAsync();
                        break;
                    case "Lesson":
                        CurrentReport = await _reportService.GenerateLessonReportAsync(ReportStartDate.Value, ReportEndDate.Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas generowania raportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToPdfAsync()
        {
            if (CurrentReport == null) return;

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"Raport_{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    switch (CurrentReport)
                    {
                        case RevenueReportDto revenue:
                            await _exportService.ExportToPdfAsync(revenue, saveDialog.FileName);
                            break;
                        case AttendanceReportDto attendance:
                            await _exportService.ExportToPdfAsync(attendance, saveDialog.FileName);
                            break;
                        case WalletReportDto wallet:
                            await _exportService.ExportToPdfAsync(wallet, saveDialog.FileName);
                            break;
                        case LessonReportDto lesson:
                            await _exportService.ExportToPdfAsync(lesson, saveDialog.FileName);
                            break;
                    }
                    MessageBox.Show("Raport został wyeksportowany do PDF.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas eksportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportToCsvAsync()
        {
            if (CurrentReport == null) return;

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Raport_{SelectedReportType}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    switch (CurrentReport)
                    {
                        case RevenueReportDto revenue:
                            await _exportService.ExportToCsvAsync(revenue, saveDialog.FileName);
                            break;
                        case AttendanceReportDto attendance:
                            await _exportService.ExportToCsvAsync(attendance, saveDialog.FileName);
                            break;
                        case WalletReportDto wallet:
                            await _exportService.ExportToCsvAsync(wallet, saveDialog.FileName);
                            break;
                        case LessonReportDto lesson:
                            await _exportService.ExportToCsvAsync(lesson, saveDialog.FileName);
                            break;
                    }
                    MessageBox.Show("Raport został wyeksportowany do CSV.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas eksportu: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
