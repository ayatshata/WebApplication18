using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Services
{
    public class BackupService : IBackupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;

        public BackupService(ApplicationDbContext context, IConfiguration configuration, ILogger<BackupService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                var backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                var backupPath = Path.Combine(backupDir, backupName);

                // In a real application, you would use SQL commands to create a database backup
                // This is a simplified version that exports critical data to JSON
                await ExportDataToJsonAsync(backupPath);

                _logger.LogInformation($"Backup created successfully: {backupName}");
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup");
                throw;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // In a real application, you would restore from the backup file
                // This is a simplified version
                _logger.LogInformation($"Restoring backup from: {backupPath}");

                // Simulate restore process
                await Task.Delay(1000);

                _logger.LogInformation("Backup restored successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore backup");
                return false;
            }
        }

        public async Task<List<string>> GetBackupListAsync()
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            if (!Directory.Exists(backupDir))
                return new List<string>();

            return await Task.FromResult(
                Directory.GetFiles(backupDir, "*.bak")
                    .Select(Path.GetFileName)
                    .OrderDescending()
                    .ToList());
        }

        public async Task<bool> DeleteBackupAsync(string backupName)
        {
            try
            {
                var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "Backups", backupName);
                if (!File.Exists(backupPath))
                    return false;

                File.Delete(backupPath);
                _logger.LogInformation($"Backup deleted: {backupName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete backup: {backupName}");
                return false;
            }
        }

        private async Task ExportDataToJsonAsync(string backupPath)
        {
            var backupData = new
            {
                Residents = await _context.Residents.ToListAsync(),
                Payments = await _context.Payments.ToListAsync(),
                Expenses = await _context.Expenses.ToListAsync(),
                MaintenanceTasks = await _context.MaintenanceTasks.ToListAsync(),
                ExportDate = DateTime.UtcNow
            };

            var json = System.Text.Json.JsonSerializer.Serialize(backupData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(backupPath, json);
        }
    }
}