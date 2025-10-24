using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [StringLength(50)]
        public string Type { get; set; } // Meeting, Event, Maintenance, Inspection

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsAllDay { get; set; } = false;

        public string CreatedByUserId { get; set; }

        [StringLength(200)]
        public string? Attendees { get; set; } // Comma-separated list of attendees

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser CreatedByUser { get; set; }
    }
}