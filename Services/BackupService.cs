using Learning_Management_System.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Learning_Management_System.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _databasePath;
        private readonly AppDbContext _dbContext;

        public BackupService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _databasePath = Path.Combine(Directory.GetCurrentDirectory(), "LearningManagementSystem.db");
        }

        public async Task BackupDatabaseAsync(string backupPath)
        {
            await Task.Run(() =>
            {
                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, backupPath, overwrite: true);
                }
                else
                {
                    throw new FileNotFoundException("Database file not found.", _databasePath);
                }
            });
        }

        public async Task RestoreDatabaseAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found.", backupPath);
            }

            await Task.Run(() =>
            {
                // Close database connection
                _dbContext.Database.CloseConnection();

                // Backup current database if it exists
                if (File.Exists(_databasePath))
                {
                    var currentBackup = _databasePath + $".backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(_databasePath, currentBackup, overwrite: true);
                }

                // Restore from backup
                File.Copy(backupPath, _databasePath, overwrite: true);
            });

            // Reconnect
            _dbContext.Database.EnsureCreated();
        }

        public async Task ExportToJsonAsync(string exportPath)
        {
            await Task.Run(async () =>
            {
                var exportData = new
                {
                    ExportDate = DateTime.Now,
                    Students = await _dbContext.Students.ToListAsync(),
                    Groups = await _dbContext.Groups
                        .Include(g => g.Schedules)
                        .Include(g => g.Enrollments)
                        .ToListAsync(),
                    Lessons = await _dbContext.Lessons.ToListAsync(),
                    Attendances = await _dbContext.Attendances.ToListAsync(),
                    Payments = await _dbContext.Payments.ToListAsync()
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                };

                var json = JsonSerializer.Serialize(exportData, options);
                await File.WriteAllTextAsync(exportPath, json);
            });
        }

        public async Task ImportFromJsonAsync(string importPath)
        {
            if (!File.Exists(importPath))
            {
                throw new FileNotFoundException("Import file not found.", importPath);
            }

            var json = await File.ReadAllTextAsync(importPath);
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            // Note: This is a simplified import. In production, you'd want more robust error handling
            // and transaction management
            var importData = JsonSerializer.Deserialize<JsonElement>(json, options);

            // Clear existing data (optional - you might want to merge instead)
            _dbContext.Students.RemoveRange(_dbContext.Students);
            _dbContext.Groups.RemoveRange(_dbContext.Groups);
            _dbContext.Lessons.RemoveRange(_dbContext.Lessons);
            _dbContext.Attendances.RemoveRange(_dbContext.Attendances);
            _dbContext.Payments.RemoveRange(_dbContext.Payments);
            await _dbContext.SaveChangesAsync();

            // Import data (simplified - would need proper deserialization in production)
            // This is a placeholder - full implementation would deserialize each entity type
            // and handle relationships properly
        }
    }
}
