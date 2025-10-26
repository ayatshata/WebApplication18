using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string Type { get; set; } = "Info"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        [StringLength(100)]
        public string? ActionUrl { get; set; }

     
        public virtual ApplicationUser User { get; set; }
    }
}