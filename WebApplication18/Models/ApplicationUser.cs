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

        public virtual ICollection<Resident> ManagedResidents { get; set; } = new List<Resident>();
        public virtual ICollection<Payment> ProcessedPayments { get; set; } = new List<Payment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

     
        public virtual ICollection<MaintenanceTask> CreatedTasks { get; set; } = new List<MaintenanceTask>();

        public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    }
}
