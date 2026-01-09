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
            var attendances = await _db.Attendances
                .Include(a => a.Lesson)
                    .ThenInclude(l => l.Group)
                .Where(a => a.Lesson.Status == LessonStatus.Completed &&
                           a.Lesson.StartTime >= start &&
                           a.Lesson.StartTime <= end)
                .ToListAsync();

            var totalRevenue = attendances.Sum(a => a.PriceCharged);

            var revenueByGroup = attendances
                .GroupBy(a => a.Lesson.Group.Name)
                .Select(g => new RevenueByGroupDto
                {
                    GroupName = g.Key,
                    Revenue = g.Sum(a => a.PriceCharged),
                    LessonCount = g.Select(a => a.LessonId).Distinct().Count()
                })
                .ToList();

            return new RevenueReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = totalRevenue,
                TotalLessons = attendances.Select(a => a.LessonId).Distinct().Count(),
                RevenueByGroup = revenueByGroup
            };
        }

        public async Task<AttendanceReportDto> GenerateAttendanceReportAsync(DateTime start, DateTime end)
        {
            var attendances = await _db.Attendances
                .Include(a => a.Student)
                .Include(a => a.Lesson)
                .Where(a => a.Lesson.StartTime >= start && a.Lesson.StartTime <= end)
                .ToListAsync();

            var attendanceByStudent = attendances
                .GroupBy(a => a.Student)
                .Select(g => new AttendanceByStudentDto
                {
                    StudentName = g.Key.FullName,
                    Present = g.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentPaid = g.Count(a => a.Status == AttendanceStatus.AbsentPaid),
                    AbsentFree = g.Count(a => a.Status == AttendanceStatus.AbsentFree),
                    Late = g.Count(a => a.Status == AttendanceStatus.Late)
                })
                .ToList();

            return new AttendanceReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present),
                TotalAbsentPaid = attendances.Count(a => a.Status == AttendanceStatus.AbsentPaid),
                TotalAbsentFree = attendances.Count(a => a.Status == AttendanceStatus.AbsentFree),
                TotalLate = attendances.Count(a => a.Status == AttendanceStatus.Late),
                AttendanceByStudent = attendanceByStudent
            };
        }

        public async Task<WalletReportDto> GenerateWalletReportAsync()
        {
            var wallets = await _paymentService.GetAllWalletsAsync();
            var students = await _db.Students
                .Where(s => s.IsActive)
                .ToListAsync();

            var studentWallets = new List<StudentWalletDto>();

            foreach (var student in students)
            {
                var balance = wallets.GetValueOrDefault(student.Id, 0);
                
                var totalPayments = await _db.Payments
                    .Where(p => p.StudentId == student.Id)
                    .SumAsync(p => p.Amount);

                var totalCharges = await _db.Attendances
                    .Include(a => a.Lesson)
                    .Where(a => a.StudentId == student.Id && a.Lesson.Status == LessonStatus.Completed)
                    .SumAsync(a => a.PriceCharged);

                studentWallets.Add(new StudentWalletDto
                {
                    StudentName = student.FullName,
                    Balance = balance,
                    TotalPayments = totalPayments,
                    TotalCharges = totalCharges
                });
            }

            return new WalletReportDto
            {
                StudentWallets = studentWallets,
                TotalPositiveBalance = studentWallets.Where(w => w.Balance > 0).Sum(w => w.Balance),
                TotalNegativeBalance = studentWallets.Where(w => w.Balance < 0).Sum(w => w.Balance)
            };
        }

        public async Task<LessonReportDto> GenerateLessonReportAsync(DateTime start, DateTime end)
        {
            var lessons = await _db.Lessons
                .Include(l => l.Group)
                .Where(l => l.StartTime >= start && l.StartTime <= end)
                .ToListAsync();

            var lessonsByGroup = lessons
                .GroupBy(l => l.Group.Name)
                .Select(g => new LessonByGroupDto
                {
                    GroupName = g.Key,
                    Total = g.Count(),
                    Completed = g.Count(l => l.Status == LessonStatus.Completed),
                    Scheduled = g.Count(l => l.Status == LessonStatus.Scheduled),
                    Canceled = g.Count(l => l.Status == LessonStatus.Canceled),
                    Holiday = g.Count(l => l.Status == LessonStatus.Holiday)
                })
                .ToList();

            return new LessonReportDto
            {
                StartDate = start,
                EndDate = end,
                TotalLessons = lessons.Count,
                CompletedLessons = lessons.Count(l => l.Status == LessonStatus.Completed),
                ScheduledLessons = lessons.Count(l => l.Status == LessonStatus.Scheduled),
                CanceledLessons = lessons.Count(l => l.Status == LessonStatus.Canceled),
                HolidayLessons = lessons.Count(l => l.Status == LessonStatus.Holiday),
                LessonsByGroup = lessonsByGroup
            };
        }
    }
}
