using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;
using MughtaribatHouse.Services;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// ✅ Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Identity Configuration
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ✅ Hangfire Configuration
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// ✅ Razor Pages + Controllers
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ✅ Register Application Services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ✅ Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ✅ Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
    }
}

// ✅ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Hangfire Dashboard (Admin Panel)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [] // يمكن لاحقًا إضافة صلاحيات للمسؤول فقط
});

// ✅ Health Check Endpoint
app.MapHealthChecks("/health");

// ✅ Docs Controller Route
app.MapControllerRoute(
    name: "docs",
    pattern: "Documents/{action=Index}/{id?}",
    defaults: new { controller = "Docs" }
);

// ✅ Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

Console.WriteLine("🚀 App is running: http://localhost:5000 or https://localhost:5001");
app.Run();
