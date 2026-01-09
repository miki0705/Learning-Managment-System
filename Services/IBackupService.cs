using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public interface IBackupService
    {
        Task BackupDatabaseAsync(string backupPath);
        Task RestoreDatabaseAsync(string backupPath);
        Task ExportToJsonAsync(string exportPath);
        Task ImportFromJsonAsync(string importPath);
    }
}
