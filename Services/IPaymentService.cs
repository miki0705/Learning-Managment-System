using Learning_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IPaymentService
    {
        Task<decimal> GetStudentWalletBalanceAsync(int studentId);
        Task<Dictionary<int, decimal>> GetAllWalletsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end);
        Task AddPaymentAsync(Payment payment);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int paymentId);
        Task UpdateWalletsFromLessonAsync(int lessonId);
    }
}
