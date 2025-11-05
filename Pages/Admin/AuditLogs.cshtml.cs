using Microsoft.AspNetCore.Mvc.RazorPages;
using MughtaribatHouse.Models;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Pages.Admin
{
    public class AuditLogsModel : PageModel
    {
        private readonly AuditService _auditService;

        public AuditLogsModel(AuditService auditService)
        {
            _auditService = auditService;
        }

        public List<AuditLog> AuditLogs { get; set; }

        public async Task OnGetAsync()
        {
            AuditLogs = await _auditService.GetAuditLogsAsync(
                fromDate: DateTime.Now.AddDays(-30));
        }
    }
}