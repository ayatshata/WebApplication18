using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.Services
{
    public class PaymentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentReminderService> _logger;

        public PaymentReminderService(IServiceProvider serviceProvider, ILogger<PaymentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    var now = DateTime.Now;

      
                    if (now.Hour == 9 && now.Minute == 0)
                    {
                        _logger.LogInformation("Running payment reminder check...");
                        await SendPaymentRemindersAsync(context, emailService);
                    }
                }
                catch (TaskCanceledException)
                {
         
                    _logger.LogWarning("PaymentReminderService task was cancelled (ignored).");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in payment reminder service.");
                }

             
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    
                }
            }

            _logger.LogInformation("PaymentReminderService stopped.");
        }

        private async Task SendPaymentRemindersAsync(ApplicationDbContext context, IEmailService emailService)
        {
            try
            {
                var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var nextMonth = currentMonth.AddMonths(1);

                var residentsWithPendingPayments = await context.Residents
                    .Where(r => r.IsActive)
                    .Where(r => !context.Payments.Any(p =>
                        p.ResidentId == r.Id && p.ForMonth == nextMonth))
                    .ToListAsync();

                foreach (var resident in residentsWithPendingPayments)
                {
                    if (!string.IsNullOrEmpty(resident.Email))
                    {
                        try
                        {
                            await emailService.SendPaymentReminderAsync(
                                resident.Email,
                                resident.FullName,
                                resident.MonthlyRent,
                                nextMonth
                            );

                            _logger.LogInformation($"Payment reminder sent to {resident.FullName}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to send payment reminder to {resident.FullName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending reminders.");
            }
        }
    }
}
