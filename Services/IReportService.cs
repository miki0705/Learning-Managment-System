using Learning_Management_System.Models;
using System;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IReportService
    {
        Task<RevenueReportDto> GenerateRevenueReportAsync(DateTime start, DateTime end);
        Task<AttendanceReportDto> GenerateAttendanceReportAsync(DateTime start, DateTime end);
        Task<WalletReportDto> GenerateWalletReportAsync();
        Task<LessonReportDto> GenerateLessonReportAsync(DateTime start, DateTime end);
    }
}
