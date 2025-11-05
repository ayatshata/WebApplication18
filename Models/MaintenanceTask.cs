using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MughtaribatHouse.Models
{
    public class MaintenanceTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string RoomNumber { get; set; }

        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; 

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; 

        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [StringLength(500)]
        public string? TechnicianNotes { get; set; }

        public string ReportedByUserId { get; set; }

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        public virtual ApplicationUser ReportedByUser { get; set; }
    }
}