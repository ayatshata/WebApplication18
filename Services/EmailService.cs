using System.Net;
using System.Net.Mail;

namespace MughtaribatHouse.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var client = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"]),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {to} with subject: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                throw;
            }
        }

        public async Task SendPaymentReminderAsync(string residentEmail, string residentName, decimal amount, DateTime dueDate)
        {
            var subject = "تذكير بدفع الإيجار - بيت المقتربات";
            var body = $"""
                <div style="font-family: Arial, sans-serif; direction: rtl; text-align: right;">
                    <h2 style="color: #2c3e50;">تذكير بدفع الإيجار</h2>
                    <p>عزيزي/عزيزتي {residentName},</p>
                    <p>نود تذكيرك بأن موعد دفع الإيجار للشهر القادم قد اقترب.</p>
                    <div style="background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;">
                        <p><strong>المبلغ:</strong> {amount:C}</p>
                        <p><strong>موعد الاستحقاق:</strong> {dueDate:yyyy/MM/dd}</p>
                    </div>
                    <p>يرجى تسديد المبلغ في أقرب وقت ممكن لتجنب أي تأخير.</p>
                    <p>شكرًا لتعاونكم,</p>
                    <p><strong>فريق إدارة بيت المقتربات</strong></p>
                </div>
                """;

            await SendEmailAsync(residentEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string residentEmail, string residentName, string roomNumber)
        {
            var subject = "مرحبًا بك في بيت المقتربات";
            var body = $"""
                <div style="font-family: Arial, sans-serif; direction: rtl; text-align: right;">
                    <h2 style="color: #27ae60;">مرحبًا بك في بيت المقتربات</h2>
                    <p>عزيزي/عزيزتي {residentName},</p>
                    <p>نرحب بك في بيت المقتربات ونتمنى لك إقامة مريحة وممتعة.</p>
                    <div style="background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;">
                        <p><strong>رقم الغرفة:</strong> {roomNumber}</p>
                        <p><strong>تاريخ التسجيل:</strong> {DateTime.Now:yyyy/MM/dd}</p>
                    </div>
                    <p>لأي استفسارات أو مساعدة، لا تتردد في التواصل مع الإدارة.</p>
                    <p>مع أطيب التحيات,</p>
                    <p><strong>فريق إدارة بيت المقتربات</strong></p>
                </div>
                """;

            await SendEmailAsync(residentEmail, subject, body);
        }

        public async Task SendMaintenanceUpdateAsync(string residentEmail, string residentName, string taskTitle, string status)
        {
            var subject = "تحديث حالة الصيانة - بيت المقتربات";
            var body = $"""
                <div style="font-family: Arial, sans-serif; direction: rtl; text-align: right;">
                    <h2 style="color: #e67e22;">تحديث حالة الصيانة</h2>
                    <p>عزيزي/عزيزتي {residentName},</p>
                    <p>نود إعلامك بتحديث حالة طلب الصيانة الخاص بك.</p>
                    <div style="background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;">
                        <p><strong>طلب الصيانة:</strong> {taskTitle}</p>
                        <p><strong>الحالة الحالية:</strong> {status}</p>
                    </div>
                    <p>سيتم إعلامك بأي تحديثات إضافية.</p>
                    <p>شكرًا لصبرك وتعاونك,</p>
                    <p><strong>فريق إدارة بيت المقتربات</strong></p>
                </div>
                """;

            await SendEmailAsync(residentEmail, subject, body);
        }
    }
}