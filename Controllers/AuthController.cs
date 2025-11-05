using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MughtaribatHouse.Models;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly AuditService _auditService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            AuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _auditService = auditService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return Error("بيانات الدخول غير صحيحة", ModelState);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                await _auditService.LogActionAsync(user.Id, "Login", "User", user.Id,
                    null, null, "User logged in", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Success(new { user.Id, user.FullName, user.Email }, "تم الدخول بنجاح");
            }

            return Error("البريد الإلكتروني أو كلمة المرور غير صحيحة");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return Error("بيانات التسجيل غير صحيحة", ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Staff");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
             

                await _auditService.LogActionAsync(user.Id, "Register", "User", user.Id,
                    null, null, "New user registered", HttpContext.Connection.RemoteIpAddress?.ToString());

                return Success(null, "تم إنشاء الحساب بنجاح");
            }

            return Error("فشل في إنشاء الحساب", result.Errors);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await _auditService.LogActionAsync(UserId, "Logout", "User", UserId,
                    null, null, "User logged out", HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            await _signInManager.SignOutAsync();
            return Success(null, "تم تسجيل الخروج بنجاح");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            { 
                
            
                return Success(null, "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
 

            await _auditService.LogActionAsync(user.Id, "ForgotPassword", "User", user.Id,
                null, null, "Password reset requested", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Success(null, "تم إرسال رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني");
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }
}