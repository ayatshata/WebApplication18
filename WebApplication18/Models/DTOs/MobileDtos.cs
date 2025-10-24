namespace MughtaribatHouse.Models.DTOs
{
    public class MobileLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class MobileAuthResponseDto
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public UserProfileDto User { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }

    public class PaymentReminderDto
    {
        public string ResidentName { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class MobileNotificationDto
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
    }

    public class ScheduleEventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public string Instructor { get; set; }
    }
}