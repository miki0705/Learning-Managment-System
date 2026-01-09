using Learning_Management_System.Models;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IExportService
    {
        Task ExportToPdfAsync(RevenueReportDto report, string filePath);
        Task ExportToPdfAsync(AttendanceReportDto report, string filePath);
        Task ExportToPdfAsync(WalletReportDto report, string filePath);
        Task ExportToPdfAsync(LessonReportDto report, string filePath);
        Task ExportToCsvAsync(RevenueReportDto report, string filePath);
        Task ExportToCsvAsync(AttendanceReportDto report, string filePath);
        Task ExportToCsvAsync(WalletReportDto report, string filePath);
        Task ExportToCsvAsync(LessonReportDto report, string filePath);
    }
}
