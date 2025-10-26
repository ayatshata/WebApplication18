using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MughtaribatHouse.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } 

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public string? ReceiptPath { get; set; }

        public string ProcessedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

     
        public virtual ApplicationUser ProcessedByUser { get; set; }
    }
}