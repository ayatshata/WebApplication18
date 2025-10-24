using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MughtaribatHouse.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public DateTime ForMonth { get; set; } // الشهر الذي يغطيه الدفع

        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? ProcessedByUserId { get; set; }

        // Navigation properties
        public virtual Resident Resident { get; set; }
        public virtual ApplicationUser? ProcessedByUser { get; set; }
    }
}