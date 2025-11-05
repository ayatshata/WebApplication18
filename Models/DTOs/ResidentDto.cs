using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models.DTOs
{
    public class ResidentDto
    {
        [Display(Name = "المعرف")]
        public int Id { get; set; }

        [Display(Name = "اسم المقيم")]
        public string Name { get; set; }

        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string? Email { get; set; }

        [Display(Name = "رقم الغرفة")]
        public string RoomNumber { get; set; }

        [Display(Name = "تاريخ التسجيل")]
        public DateTime CheckInDate { get; set; }

        [Display(Name = "تاريخ المغادرة")]
        public DateTime? CheckOutDate { get; set; }

        [Display(Name = "الإيجار الشهري")]
        public decimal MonthlyRent { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }

        [Display(Name = "إجمالي المدفوع")]
        public decimal TotalPaid { get; set; }

        [Display(Name = "آخر تاريخ دفع")]
        public DateTime? LastPaymentDate { get; set; }

        [Display(Name = "أيام منذ التسجيل")]
        public int DaysSinceCheckIn { get; set; }
    }

    public class CreateResidentDto
    {
        [Required(ErrorMessage = "اسم المقيم مطلوب")]
        [Display(Name = "اسم المقيم")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن لا يتجاوز 100 حرف")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [StringLength(20, ErrorMessage = "رقم الهاتف يجب أن لا يتجاوز 20 رقم")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "رقم الغرفة مطلوب")]
        [Display(Name = "رقم الغرفة")]
        [StringLength(50, ErrorMessage = "رقم الغرفة يجب أن لا يتجاوز 50 حرف")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "تاريخ التسجيل مطلوب")]
        [Display(Name = "تاريخ التسجيل")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "الإيجار الشهري مطلوب")]
        [Display(Name = "الإيجار الشهري")]
        [Range(0, double.MaxValue, ErrorMessage = "قيمة الإيجار يجب أن تكون موجبة")]
        public decimal MonthlyRent { get; set; }
    }

    public class UpdateResidentDto
    {
        [Required(ErrorMessage = "اسم المقيم مطلوب")]
        [Display(Name = "اسم المقيم")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن لا يتجاوز 100 حرف")]
        public string Name { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        [StringLength(20, ErrorMessage = "رقم الهاتف يجب أن لا يتجاوز 20 رقم")]
        [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "رقم الغرفة مطلوب")]
        [Display(Name = "رقم الغرفة")]
        [StringLength(50, ErrorMessage = "رقم الغرفة يجب أن لا يتجاوز 50 حرف")]
        public string RoomNumber { get; set; }

        [Required(ErrorMessage = "الإيجار الشهري مطلوب")]
        [Display(Name = "الإيجار الشهري")]
        [Range(0, double.MaxValue, ErrorMessage = "قيمة الإيجار يجب أن تكون موجبة")]
        public decimal MonthlyRent { get; set; }
    }
}