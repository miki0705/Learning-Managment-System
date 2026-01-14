using Learning_Management_System.Data;
using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        private readonly IPaymentService _paymentService;

        public ReportService(AppDbContext db, IPaymentService paymentService)
        {
            _db = db;
            _paymentService = paymentService;
        }

        public async Task<RevenueReportDto> GenerateRevenueReportAsync(DateTime start, DateTime end)
        {
            // Use IgnoreQueryFilters to include soft-deleted records for historical accuracy
            var attendances = await _db.Attendances
                .IgnoreQueryFilters()
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Include(a => a.Student)
                .Where(a => !a.IsDeleted &&
                           a.Lesson.Status == LessonStatus.Completed &&
                           a.Lesson.StartTime >= start &&
                           a.Lesson.StartTime <= end &&
                           !a.Lesson.IsDeleted &&
                           !a.Student.IsDeleted)
                .OrderBy(a => a.Lesson.StartTime)
                .ThenBy(a => a.Student.LastName)
                .ToListAsync();

            var transactions = attendances.Select(a => new RevenueTransactionDto
            {
                AttendanceId = a.Id,
                LessonId = a.LessonId,
                LessonDate = a.Lesson.StartTime.Date,
                GroupName = a.Lesson.Group.Name,
                StudentName = a.Student.FullName,
                PriceCharged = a.PriceCharged,
                Status = a.Status
            }).ToList();

            return new RevenueReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = attendances.Sum(a => a.PriceCharged),
                TotalLessons = attendances.Select(a => a.LessonId).Distinct().Count(),
                Transactions = transactions
            };
        }

        public async Task<AttendanceReportDto> GenerateAttendanceReportAsync(DateTime start, DateTime end)
        {
            // Use IgnoreQueryFilters to include soft-deleted records for historical accuracy
            var attendances = await _db.Attendances
                .IgnoreQueryFilters()
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => !a.IsDeleted &&
                           a.Lesson.StartTime >= start && 
                           a.Lesson.StartTime <= end &&
                           !a.Lesson.IsDeleted &&
                           !a.Student.IsDeleted)
                .OrderBy(a => a.Lesson.StartTime)
                .ThenBy(a => a.Student.LastName)
                .ToListAsync();

            var entries = attendances.Select(a => new AttendanceEntryDto
            {
                AttendanceId = a.Id,
                LessonId = a.LessonId,
                LessonDate = a.Lesson.StartTime.Date,
                GroupName = a.Lesson.Group.Name,
                StudentName = a.Student.FullName,
                Status = a.Status,
                PriceCharged = a.PriceCharged
            }).ToList();

            return new AttendanceReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present),
                TotalAbsentPaid = attendances.Count(a => a.Status == AttendanceStatus.AbsentPaid),
                TotalAbsentFree = attendances.Count(a => a.Status == AttendanceStatus.AbsentFree),
                TotalLate = attendances.Count(a => a.Status == AttendanceStatus.Late),
                Entries = entries
            };
        }

        public async Task<WalletReportDto> GenerateWalletReportAsync()
        {
            // Use IgnoreQueryFilters to include soft-deleted records for historical accuracy
            var payments = await _db.Payments
                .IgnoreQueryFilters()
                .Include(p => p.Student)
                .Where(p => !p.IsDeleted && !p.Student.IsDeleted)
                .OrderBy(p => p.Date)
                .ThenBy(p => p.Student.LastName)
                .ToListAsync();

            var charges = await _db.Attendances
                .IgnoreQueryFilters()
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => !a.IsDeleted &&
                           a.Lesson.Status == LessonStatus.Completed &&
                           !a.Lesson.IsDeleted &&
                           !a.Student.IsDeleted)
                .OrderBy(a => a.Lesson.StartTime)
                .ThenBy(a => a.Student.LastName)
                .ToListAsync();

            var transactions = new List<WalletTransactionDto>();

            // Add all payments
            foreach (var payment in payments)
            {
                transactions.Add(new WalletTransactionDto
                {
                    TransactionId = payment.Id,
                    TransactionType = "Payment",
                    TransactionDate = payment.Date,
                    StudentName = payment.Student.FullName,
                    Amount = payment.Amount,
                    Description = payment.Description ?? "Wpłata",
                    PaymentId = payment.Id
                });
            }

            // Add all charges
            foreach (var charge in charges)
            {
                transactions.Add(new WalletTransactionDto
                {
                    TransactionId = charge.Id,
                    TransactionType = "Charge",
                    TransactionDate = charge.Lesson.StartTime.Date,
                    StudentName = charge.Student.FullName,
                    Amount = -charge.PriceCharged, // Negative for charges
                    Description = $"Opłata za lekcję - {charge.Lesson.Group.Name}",
                    LessonId = charge.LessonId
                });
            }

            // Sort by date
            transactions = transactions.OrderBy(t => t.TransactionDate).ThenBy(t => t.StudentName).ToList();

            var wallets = await _paymentService.GetAllWalletsAsync();
            var totalPositive = wallets.Values.Where(b => b > 0).Sum();
            var totalNegative = wallets.Values.Where(b => b < 0).Sum();

            return new WalletReportDto
            {
                GeneratedDate = DateTime.Now,
                TotalPositiveBalance = totalPositive,
                TotalNegativeBalance = totalNegative,
                Transactions = transactions
            };
        }

        public async Task<LessonReportDto> GenerateLessonReportAsync(DateTime start, DateTime end)
        {
            // Use IgnoreQueryFilters to include soft-deleted records for historical accuracy
            var lessons = await _db.Lessons
                .IgnoreQueryFilters()
                .Include(l => l.Group)
                .Include(l => l.Attendances)
                .Where(l => !l.IsDeleted &&
                           l.StartTime >= start && 
                           l.StartTime <= end &&
                           !l.Group.IsDeleted)
                .OrderBy(l => l.StartTime)
                .ToListAsync();

            var lessonEntries = lessons.Select(l => new LessonEntryDto
            {
                LessonId = l.Id,
                LessonDate = l.StartTime.Date,
                StartTime = l.StartTime,
                EndTime = l.EndTime,
                GroupName = l.Group.Name,
                Status = l.Status,
                Note = l.Note,
                AttendanceCount = l.Attendances.Count
            }).ToList();

            return new LessonReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalLessons = lessons.Count,
                CompletedLessons = lessons.Count(l => l.Status == LessonStatus.Completed),
                ScheduledLessons = lessons.Count(l => l.Status == LessonStatus.Scheduled),
                CanceledLessons = lessons.Count(l => l.Status == LessonStatus.Canceled),
                HolidayLessons = lessons.Count(l => l.Status == LessonStatus.Holiday),
                Lessons = lessonEntries
            };
        }
    }
}
