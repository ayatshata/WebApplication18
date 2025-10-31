using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MughtaribatHouse.Models
{
    public class Resident
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [StringLength(20)]
        public string IdentityNumber { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(10)]
        public string RoomNumber { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyRent { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public string? ManagedByUserId { get; set; }
        public virtual ApplicationUser? ManagedByUser { get; set; }
    }
}