using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MughtaribatHouse.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المستند مطلوب")]
        [StringLength(200, ErrorMessage = "العنوان يجب ألا يتجاوز 200 حرف")]
        [Display(Name = "عنوان المستند")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "مسار الملف مطلوب")]
        [Display(Name = "مسار الملف")]
        public string FilePath { get; set; }

        [Required(ErrorMessage = "نوع الملف مطلوب")]
        [StringLength(50, ErrorMessage = "نوع الملف يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "نوع الملف")]
        public string FileType { get; set; }

        [Display(Name = "حجم الملف")]
        public long FileSize { get; set; }

        [Required(ErrorMessage = "التصنيف مطلوب")]
        [StringLength(50, ErrorMessage = "التصنيف يجب ألا يتجاوز 50 حرف")]
        [Display(Name = "التصنيف")]
        public string Category { get; set; } = "Other";

        [Display(Name = "المقيم")]
        public int? ResidentId { get; set; }

        [Required(ErrorMessage = "المستخدم الذي رفع الملف مطلوب")]
        [Display(Name = "مرفوع بواسطة")]
        public string UploadedByUserId { get; set; }

        [Display(Name = "تاريخ الرفع")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاريخ التعديل")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "عام")]
        public bool IsPublic { get; set; } = false;

        [StringLength(50)]
        [Display(Name = "حالة الملف")]
        public string Status { get; set; } = "Active"; 

        [StringLength(500)]
        [Display(Name = "العلامات")]
        public string? Tags { get; set; }


        [Display(Name = "المقيم")]
        public virtual Resident? Resident { get; set; }

        [Display(Name = "مرفوع بواسطة")]
        public virtual ApplicationUser UploadedByUser { get; set; }

        [NotMapped]
        [Display(Name = "حجم الملف")]
        public string FileSizeDisplay => GetFileSizeDisplay();

        [NotMapped]
        [Display(Name = "صورة")]
        public bool IsImage => FileType?.ToLower() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => true,
            _ => false
        };

        [NotMapped]
        [Display(Name = "PDF")]
        public bool IsPdf => FileType?.ToLower() == ".pdf";

        [NotMapped]
        [Display(Name = "مستند")]
        public bool IsDocument => FileType?.ToLower() switch
        {
            ".doc" or ".docx" or ".txt" or ".rtf" => true,
            _ => false
        };

        [NotMapped]
        [Display(Name = "جدول بيانات")]
        public bool IsSpreadsheet => FileType?.ToLower() switch
        {
            ".xls" or ".xlsx" or ".csv" => true,
            _ => false
        };

      
        private string GetFileSizeDisplay()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}