using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;

        public PaymentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<decimal> GetStudentWalletBalanceAsync(int studentId)
        {
            // Sum of all payments
            decimal totalPayments = await _db.Payments
                .Where(p => p.StudentId == studentId)
                .SumAsync(p => p.Amount);

            // Sum of all charges from completed lessons
            decimal totalCharges = await _db.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Include(a => a.Student)
                    .ThenInclude(s => s.Enrollments)
                .Where(a => a.StudentId == studentId && 
                           a.Lesson.Status == LessonStatus.Completed)
                .SumAsync(a => a.PriceCharged);

            return totalPayments - totalCharges;
        }

        public async Task<Dictionary<int, decimal>> GetAllWalletsAsync()
        {
            var wallets = new Dictionary<int, decimal>();
            var studentIds = await _db.Students
                .Where(s => s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            foreach (var studentId in studentIds)
            {
                wallets[studentId] = await GetStudentWalletBalanceAsync(studentId);
            }

            return wallets;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId)
        {
            return await _db.Payments
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Payments
                .Include(p => p.Student)
                .Where(p => p.Date >= start && p.Date <= end)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            var trackedEntity = _db.Payments.Local.FirstOrDefault(p => p.Id == payment.Id);
            if (trackedEntity != null)
            {
                _db.Entry(trackedEntity).State = EntityState.Detached;
            }

            _db.Entry(payment).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment != null)
            {
                _db.Payments.Remove(payment);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateWalletsFromLessonAsync(int lessonId)
        {
            var lesson = await _db.Lessons
                .Include(l => l.Group)
                .Include(l => l.Attendances)
                    .ThenInclude(a => a.Student)
                        .ThenInclude(s => s.Enrollments)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null || lesson.Status != LessonStatus.Completed)
                return;

            foreach (var attendance in lesson.Attendances)
            {
                // Calculate price based on attendance status
                decimal priceToCharge = 0;

                if (attendance.Status == AttendanceStatus.Present || attendance.Status == AttendanceStatus.AbsentPaid)
                {
                    // Get enrollment to check for individual rate
                    var enrollment = attendance.Student.Enrollments
                        .FirstOrDefault(e => e.GroupId == lesson.GroupId);

                    if (enrollment?.IndividualRate.HasValue == true)
                    {
                        priceToCharge = enrollment.IndividualRate.Value;
                    }
                    else
                    {
                        priceToCharge = lesson.Group.BaseRate;
                    }
                }
                // AbsentFree and Late (if Late is free) = 0

                // Update attendance with calculated price
                attendance.PriceCharged = priceToCharge;
            }

            await _db.SaveChangesAsync();
        }
    }
}
