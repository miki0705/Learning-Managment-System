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
    public class FinanceViewModel : ViewModelBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IStudentService _studentService;

        private Payment? _selectedPayment;
        private Student? _selectedStudent;
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;
        private string _searchText = string.Empty;
        private bool _isDirty;
        private bool _isInternalUpdate = false;
        private Dictionary<int, decimal> _walletBalances = new();

        public FinanceViewModel(IPaymentService paymentService, IStudentService studentService)
        {
            _paymentService = paymentService;
            _studentService = studentService;

            Students = new ObservableCollection<Student>(_studentService.GetAllStudents());
            Payments = new ObservableCollection<Payment>();
            WalletBalances = new ObservableCollection<StudentWalletViewModel>();

            AddPaymentCommand = new RelayCommand(o => AddPayment(), o => !IsDirty);
            SavePaymentCommand = new RelayCommand(async o => await SavePaymentAsync(), o => IsDirty && SelectedPayment != null);
            DeletePaymentCommand = new RelayCommand(async o => await DeletePaymentAsync(), o => SelectedPayment != null && !IsDirty);
            CancelPaymentCommand = new RelayCommand(o => CancelPaymentChanges(), o => IsDirty && SelectedPayment != null);

            _ = LoadPaymentsAsync();
            _ = LoadWalletBalancesAsync();
        }

        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Payment> Payments { get; set; }
        public ObservableCollection<StudentWalletViewModel> WalletBalances { get; set; }

        public Payment? SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                if (_selectedPayment == value) return;
                if (_selectedPayment != null)
                {
                    _selectedPayment.PropertyChanged -= OnPaymentPropertyChanged;
                    // Remove handler for Student property if it exists
                }
                _selectedPayment = value;
                if (_selectedPayment != null)
                {
                    _selectedPayment.PropertyChanged += OnPaymentPropertyChanged;
                    // Ensure StudentId is synced with Student
                    if (_selectedPayment.Student != null && _selectedPayment.StudentId != _selectedPayment.Student.Id)
                    {
                        _selectedPayment.StudentId = _selectedPayment.Student.Id;
                    }
                }
                OnPropertyChanged();
                IsDirty = false;
            }
        }

        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent == value) return;
                _selectedStudent = value;
                OnPropertyChanged();
                _ = LoadPaymentsAsync();
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
                _ = LoadPaymentsAsync();
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
                _ = LoadPaymentsAsync();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddPaymentCommand { get; }
        public ICommand SavePaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }
        public ICommand CancelPaymentCommand { get; }

        private void OnPaymentPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isInternalUpdate) return;
            
            // Sync StudentId when Student changes
            if (e.PropertyName == nameof(Payment.Student) && sender is Payment payment)
            {
                if (payment.Student != null)
                {
                    payment.StudentId = payment.Student.Id;
                }
            }
            
            IsDirty = true;
        }

        private void AddPayment()
        {
            var newPayment = new Payment
            {
                StudentId = SelectedStudent?.Id ?? 0,
                Student = SelectedStudent,
                Date = DateTime.Today,
                Amount = 0,
                Description = string.Empty
            };
            Payments.Add(newPayment);
            SelectedPayment = newPayment;
            IsDirty = true;
        }

        private async Task SavePaymentAsync()
        {
            if (SelectedPayment == null) return;

            if (SelectedPayment.Id == 0)
                await _paymentService.AddPaymentAsync(SelectedPayment);
            else
                await _paymentService.UpdatePaymentAsync(SelectedPayment);

            IsDirty = false;
            await LoadPaymentsAsync();
            await LoadWalletBalancesAsync();
        }

        private void CancelPaymentChanges()
        {
            if (SelectedPayment == null) return;
            _isInternalUpdate = true;
            if (SelectedPayment.Id == 0)
            {
                Payments.Remove(SelectedPayment);
                SelectedPayment = null;
            }
            else
            {
                // Reload payment from database
                var temp = SelectedPayment;
                SelectedPayment = null;
                _ = LoadPaymentsAsync();
            }
            _isInternalUpdate = false;
            IsDirty = false;
        }

        private async Task DeletePaymentAsync()
        {
            if (SelectedPayment == null) return;
            if (MessageBox.Show($"Usunąć płatność {SelectedPayment.Amount} PLN?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (SelectedPayment.Id > 0)
                {
                    await _paymentService.DeletePaymentAsync(SelectedPayment.Id);
                }
                Payments.Remove(SelectedPayment);
                SelectedPayment = null;
                IsDirty = false;
                await LoadWalletBalancesAsync();
            }
        }

        private async Task LoadPaymentsAsync()
        {
            IEnumerable<Payment> payments;

            if (SelectedStudent != null)
            {
                payments = await _paymentService.GetPaymentsByStudentAsync(SelectedStudent.Id);
            }
            else if (FilterStartDate.HasValue && FilterEndDate.HasValue)
            {
                payments = await _paymentService.GetPaymentsByDateRangeAsync(FilterStartDate.Value, FilterEndDate.Value);
            }
            else
            {
                // Load all payments
                payments = await _paymentService.GetPaymentsByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
            }

            Payments.Clear();
            foreach (var payment in payments)
            {
                // Ensure Student is loaded
                if (payment.Student == null && payment.StudentId > 0)
                {
                    payment.Student = Students.FirstOrDefault(s => s.Id == payment.StudentId);
                }
                Payments.Add(payment);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                CollectionViewSource.GetDefaultView(Payments)?.Refresh();
                return;
            }

            var view = CollectionViewSource.GetDefaultView(Payments);
            view.Filter = item =>
            {
                if (item is Payment payment)
                {
                    return payment.Student?.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                           payment.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                           payment.Amount.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
        }

        private async Task LoadWalletBalancesAsync()
        {
            _walletBalances = await _paymentService.GetAllWalletsAsync();

            WalletBalances.Clear();
            foreach (var student in Students)
            {
                var balance = _walletBalances.GetValueOrDefault(student.Id, 0);
                WalletBalances.Add(new StudentWalletViewModel
                {
                    Student = student,
                    Balance = balance
                });
            }

            // Sort by balance (negative first)
            var sorted = WalletBalances.OrderBy(w => w.Balance).ToList();
            WalletBalances.Clear();
            foreach (var item in sorted)
            {
                WalletBalances.Add(item);
            }
        }
    }

    public class StudentWalletViewModel : ViewModelBase
    {
        private Student _student = null!;
        private decimal _balance;

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

        public decimal Balance
        {
            get => _balance;
            set
            {
                if (_balance == value) return;
                _balance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNegative));
                OnPropertyChanged(nameof(BalanceDisplay));
            }
        }

        public bool IsNegative => Balance < 0;

        public string BalanceDisplay => $"{Balance:F2} PLN";
    }
}
