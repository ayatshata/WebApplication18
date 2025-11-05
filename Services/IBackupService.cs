namespace MughtaribatHouse.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupPath);
        Task<List<string>> GetBackupListAsync();
        Task<bool> DeleteBackupAsync(string backupName);
    }
}