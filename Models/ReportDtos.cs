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
        public List<RevenueTransactionDto> Transactions { get; set; } = new();
    }

    public class RevenueTransactionDto
    {
        public int AttendanceId { get; set; }
        public int LessonId { get; set; }
        public DateTime LessonDate { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal PriceCharged { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class AttendanceReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsentPaid { get; set; }
        public int TotalAbsentFree { get; set; }
        public int TotalLate { get; set; }
        public List<AttendanceEntryDto> Entries { get; set; } = new();
    }

    public class AttendanceEntryDto
    {
        public int AttendanceId { get; set; }
        public int LessonId { get; set; }
        public DateTime LessonDate { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public AttendanceStatus Status { get; set; }
        public decimal PriceCharged { get; set; }
    }

    public class WalletReportDto
    {
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public decimal TotalPositiveBalance { get; set; }
        public decimal TotalNegativeBalance { get; set; }
        public List<WalletTransactionDto> Transactions { get; set; } = new();
    }

    public class WalletTransactionDto
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Payment" or "Charge"
        public DateTime TransactionDate { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? LessonId { get; set; } // For charges
        public int? PaymentId { get; set; } // For payments
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
        public List<LessonEntryDto> Lessons { get; set; } = new();
    }

    public class LessonEntryDto
    {
        public int LessonId { get; set; }
        public DateTime LessonDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public LessonStatus Status { get; set; }
        public string? Note { get; set; }
        public int AttendanceCount { get; set; }
    }
}
