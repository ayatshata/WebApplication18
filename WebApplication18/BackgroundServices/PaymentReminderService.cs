using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;

namespace MughtaribatHouse.BackgroundServices
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
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Check for pending payments every day at 9 AM
                    var now = DateTime.Now;
                    if (now.Hour == 9 && now.Minute == 0)
                    {
                        await CheckPendingPayments(context);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in payment reminder service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task CheckPendingPayments(ApplicationDbContext context)
        {
            var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var nextMonth = currentMonth.AddMonths(1);

            var residentsWithPendingPayments = await context.Residents
                .Where(r => r.IsActive)
                .Where(r => !context.Payments.Any(p =>
                    p.ResidentId == r.Id && p.ForMonth == nextMonth))
                .ToListAsync();

            _logger.LogInformation($"Found {residentsWithPendingPayments.Count} residents with pending payments");


        }
    }
}