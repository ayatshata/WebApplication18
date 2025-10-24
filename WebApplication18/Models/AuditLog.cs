using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } // Create, Update, Delete, Login, etc.

        [Required]
        [StringLength(100)]
        public string Entity { get; set; } // Resident, Payment, etc.

        public string? EntityId { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
    }
}