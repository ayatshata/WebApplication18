namespace MughtaribatHouse.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPaymentReminderAsync(string residentEmail, string residentName, decimal amount, DateTime dueDate);
        Task SendWelcomeEmailAsync(string residentEmail, string residentName, string roomNumber);
        Task SendMaintenanceUpdateAsync(string residentEmail, string residentName, string taskTitle, string status);
    }
}