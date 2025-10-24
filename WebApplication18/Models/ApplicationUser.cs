using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public string? ProfilePicture { get; set; }

        // Navigation properties
        public virtual ICollection<Resident> ManagedResidents { get; set; }
        public virtual ICollection<Payment> ProcessedPayments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<MaintenanceTask> CreatedTasks { get; set; }
    }
}