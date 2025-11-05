using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public bool IsPresent { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public string RecordedByUserId { get; set; }

 
        public virtual Resident Resident { get; set; }
        public virtual ApplicationUser RecordedByUser { get; set; }
    }
}