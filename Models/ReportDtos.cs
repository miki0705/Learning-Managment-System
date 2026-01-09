using System;
using System.Collections.Generic;

namespace Learning_Management_System.Models
{
    public class RevenueReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalLessons { get; set; }
        public List<RevenueByGroupDto> RevenueByGroup { get; set; } = new();
    }

    public class RevenueByGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int LessonCount { get; set; }
    }

    public class AttendanceReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsentPaid { get; set; }
        public int TotalAbsentFree { get; set; }
        public int TotalLate { get; set; }
        public List<AttendanceByStudentDto> AttendanceByStudent { get; set; } = new();
    }

    public class AttendanceByStudentDto
    {
        public string StudentName { get; set; } = string.Empty;
        public int Present { get; set; }
        public int AbsentPaid { get; set; }
        public int AbsentFree { get; set; }
        public int Late { get; set; }
    }

    public class WalletReportDto
    {
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public List<StudentWalletDto> StudentWallets { get; set; } = new();
        public decimal TotalPositiveBalance { get; set; }
        public decimal TotalNegativeBalance { get; set; }
    }

    public class StudentWalletDto
    {
        public string StudentName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalCharges { get; set; }
    }

    public class LessonReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int ScheduledLessons { get; set; }
        public int CanceledLessons { get; set; }
        public int HolidayLessons { get; set; }
        public List<LessonByGroupDto> LessonsByGroup { get; set; } = new();
    }

    public class LessonByGroupDto
    {
        public string GroupName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Scheduled { get; set; }
        public int Canceled { get; set; }
        public int Holiday { get; set; }
    }
}
