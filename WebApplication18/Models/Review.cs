using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ReviewerName { get; set; }

        [EmailAddress]
        public string? ReviewerEmail { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(20)]
        public string? ResidentType { get; set; } // Current, Former, Family

        public bool WouldRecommend { get; set; } = true;
    }
}