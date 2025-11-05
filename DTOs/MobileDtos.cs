using System.ComponentModel.DataAnnotations;

namespace MughtaribatHouse.Models.DTOs
{
    public class MobileLoginDto1
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; }
    }

    public class MobileAuthResponse
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public UserInfo User { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string ProfilePicture { get; set; }
    }

    public class MobilePaymentDto
    {
        [Required(ErrorMessage = "معرف المقيم مطلوب")]
        public int ResidentId { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "تاريخ الدفع مطلوب")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
        public string PaymentMethod { get; set; } = "Cash";

        public string Notes { get; set; }
    }

    public class MobileMaintenanceRequest
    {
        [Required(ErrorMessage = "عنوان الطلب مطلوب")]
        [StringLength(100, ErrorMessage = "العنوان يجب ألا يتجاوز 100 حرف")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        [StringLength(500, ErrorMessage = "الوصف يجب ألا يتجاوز 500 حرف")]
        public string Description { get; set; }

        [Required(ErrorMessage = "رقم الغرفة مطلوب")]
        public string RoomNumber { get; set; }

        public string Priority { get; set; } = "Medium";
    }

    public class MobileNotificationDto1
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string ActionUrl { get; set; }
        public string Icon { get; set; }
        public string BadgeColor { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [Compare("NewPassword", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب ألا يتجاوز 100 حرف")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "رقم الجوال غير صحيح")]
        public string PhoneNumber { get; set; }

        public string ProfilePicture { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class MobileResidentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    public class MobilePaymentHistoryDto
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ForMonth { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public string ProcessedBy { get; set; }
    }

    public class MobileDashboardDto
    {
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PendingPayments { get; set; }
        public int UnreadNotifications { get; set; }
        public List<MobilePaymentHistoryDto> RecentPayments { get; set; } = new List<MobilePaymentHistoryDto>();
        public List<MobileNotificationDto> RecentNotifications { get; set; } = new List<MobileNotificationDto>();
    }
}