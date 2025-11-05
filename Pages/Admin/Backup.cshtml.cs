using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Pages.Admin
{
    public class BackupModel : PageModel
    {
        private readonly IBackupService _backupService;

        public BackupModel(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public List<string> BackupFiles { get; set; }

        public async Task OnGetAsync()
        {
            BackupFiles = await _backupService.GetBackupListAsync();
        }
    }
}