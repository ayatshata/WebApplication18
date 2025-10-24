using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MughtaribatHouse.Data;
using MughtaribatHouse.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔧 إعداد المنفذ للتجربة المحلية
builder.WebHost.UseUrls("http://localhost:5050");

// 🔹 اتصال قاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 🔹 Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🔹 MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// 🔹 قاعدة البيانات والتهيئة
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(context, userManager, roleManager);
        Console.WriteLine("✅ قاعدة البيانات جاهزة");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ خطأ أثناء التهيئة: {ex}");
    }
}

// 🔹 Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection(); // ممكن تحذفه لو تريد HTTP فقط
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Razor Pages + Controllers
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 🔹 صفحة رئيسية بسيطة
app.MapGet("/", async (HttpContext context) =>
{
    var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
    var html = $@"
    <!DOCTYPE html>
    <html lang='ar' dir='rtl'>
    <head>
        <meta charset='utf-8'>
        <title>بيت المقتربات</title>
        <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
    </head>
    <body class='p-5 text-center'>
        <h1 class='text-primary'>🏠 بيت المقتربات</h1>
        {(isAuthenticated ? "<div class='alert alert-success'>✅ التطبيق جاهز للعمل</div>" :
                            "<div class='alert alert-info'>سجل الدخول للبدء</div>")}
    </body>
    </html>";

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(html);
});

// 🔹 صحة التطبيق
app.MapGet("/health", () => Results.Json(new
{
    status = "Healthy",
    port = 5050,
    timestamp = DateTime.Now
}));

Console.WriteLine("🚀 التطبيق بدأ:");
Console.WriteLine("📍 http://localhost:5050");

app.Run();
