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
            try
            {
                // Apply any pending migrations instead of EnsureCreated
                await context.Database.MigrateAsync();

                // Create roles
                string[] roleNames = { "Admin", "Manager", "Staff", "Resident" };

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Create admin user
                var adminUser = await userManager.FindByEmailAsync("admin@mughtaribathouse.com");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin@mughtaribathouse.com",
                        Email = "admin@mughtaribathouse.com",
                        FullName = "System Administrator",
                        PhoneNumber = "+966500000000",
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // Check if Residents table exists and is empty
                if (await context.Residents.AnyAsync())
                {
                    return; // Data already seeded
                }

                // Create sample residents
                var residents = new List<Resident>
                {
                    new Resident
                    {
                        FullName = "أحمد محمد",
                        IdentityNumber = "1234567890",
                        PhoneNumber = "+966500000001",
                        Email = "ahmed@example.com",
                        RoomNumber = "101",
                        CheckInDate = DateTime.Now.AddMonths(-3),
                        MonthlyRent = 1500.00m,
                        IsActive = true,
                        Notes = "مقيم منتظم في السداد",
                        ManagedByUserId = adminUser.Id
                    },
                    new Resident
                    {
                        FullName = "فاطمة عبدالله",
                        IdentityNumber = "1234567891",
                        PhoneNumber = "+966500000002",
                        Email = "fatima@example.com",
                        RoomNumber = "102",
                        CheckInDate = DateTime.Now.AddMonths(-1),
                        MonthlyRent = 1500.00m,
                        IsActive = true,
                        Notes = "مقيمة جديدة",
                        ManagedByUserId = adminUser.Id
                    }
                };

                await context.Residents.AddRangeAsync(residents);
                await context.SaveChangesAsync();

                // Create sample payments
                var resident = await context.Residents.FirstOrDefaultAsync();
                if (resident != null)
                {
                    var payments = new List<Payment>
                    {
                        new Payment
                        {
                            ResidentId = resident.Id,
                            Amount = 1500.00m,
                            PaymentDate = DateTime.Now.AddDays(-5),
                            ForMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                            PaymentMethod = "Cash",
                            Notes = "دفع نقدي",
                            ProcessedByUserId = adminUser.Id
                        }
                    };

                    await context.Payments.AddRangeAsync(payments);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error (you should use proper logging here)
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
    }
}