using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Models;

namespace MughtaribatHouse.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
         
            await context.Database.MigrateAsync();

            var roles = new[] { "Admin", "Manager", "Staff", "Resident" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            var admin = await userManager.FindByEmailAsync("admin@mughtaribathouse.com");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin@mughtaribathouse.com",
                    Email = "admin@mughtaribathouse.com",
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };
                var res = await userManager.CreateAsync(admin, "Admin123!");
                if (res.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (!await context.Residents.AnyAsync())
            {
                var r1 = new Resident
                {
                    FullName = "أحمد محمد",
                    IdentityNumber = "1234567890",
                    PhoneNumber = "+966500000001",
                    Email = "ahmed@example.com",
                    RoomNumber = "101",
                    CheckInDate = DateTime.Now.AddMonths(-3),
                    MonthlyRent = 1500m,
                    IsActive = true,
                    Notes = "مقيم منتظم",
                    ManagedByUserId = admin.Id
                };
                var r2 = new Resident
                {
                    FullName = "فاطمة عبدالله",
                    IdentityNumber = "1234567891",
                    PhoneNumber = "+966500000002",
                    Email = "fatima@example.com",
                    RoomNumber = "102",
                    CheckInDate = DateTime.Now.AddMonths(-1),
                    MonthlyRent = 1500m,
                    IsActive = true,
                    Notes = "مقيمة جديدة",
                    ManagedByUserId = admin.Id
                };
                await context.Residents.AddRangeAsync(r1, r2);
                await context.SaveChangesAsync();

                var payments = new[]
                {
                    new Payment {
                        ResidentId = r1.Id,
                        Amount = r1.MonthlyRent,
                        PaymentDate = DateTime.Now.AddDays(-5),
                        ForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month,1),
                        PaymentMethod = "Cash",
                        Notes = "دفعة تجريبية",
                        ProcessedByUserId = admin.Id
                    },
                    new Payment {
                        ResidentId = r2.Id,
                        Amount = r2.MonthlyRent,
                        PaymentDate = DateTime.Now.AddDays(-4),
                        ForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month,1),
                        PaymentMethod = "Card",
                        Notes = "دفعة تجريبية",
                        ProcessedByUserId = admin.Id
                    }
                };
                await context.Payments.AddRangeAsync(payments);

                var tasks = new[]
                {
                    new MaintenanceTask {
                        Title = "تصليح المكيف",
                        Description = "المكيف لا يعمل بالكامل، يحتاج تنظيف فلتر وفحص الغاز.",
                        RoomNumber = "101",
                        Priority = "High",
                        ReportedDate = DateTime.Now.AddDays(-2),
                        ReportedByUserId = admin.Id,
                        Status = "Pending"
                    },
                    new MaintenanceTask {
                        Title = "إصلاح السباكة",
                        Description = "تسرّب ماء من أنبوب الحمام.",
                        RoomNumber = "102",
                        Priority = "Medium",
                        ReportedDate = DateTime.Now.AddDays(-1),
                        ReportedByUserId = admin.Id,
                        Status = "Pending"
                    }
                };
                await context.MaintenanceTasks.AddRangeAsync(tasks);

              //كانت ايرور
                var attendances = new[]
                {
                    new Attendance { ResidentId = r1.Id, Date = DateTime.Now.Date, IsPresent = true, RecordedByUserId = admin.Id },
                    new Attendance { ResidentId = r2.Id, Date = DateTime.Now.Date, IsPresent = true, RecordedByUserId = admin.Id }
                };
                await context.Attendances.AddRangeAsync(attendances);

                // Notifications
                var notifications = new[]
                {
                    new Notification { Title = "تذكير بدفع الإيجار", Message = "الرجاء دفع الإيجار.", UserId = admin.Id, CreatedAt = DateTime.Now },
                    new Notification { Title = "مرحباً", Message = "مرحباً بكم في النظام.", UserId = admin.Id, CreatedAt = DateTime.Now }
                };
                await context.Notifications.AddRangeAsync(notifications);

                await context.SaveChangesAsync();
            }
        }
    }
}
