using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MughtaribatHouse.Models;
using MughtaribatHouse.Models.DTOs;
using MughtaribatHouse.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MughtaribatHouse.Controllers
{
    [Route("api/mobile")]
    [ApiController]
    public class MobileApiController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IResidentService _residentService;
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public MobileApiController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IResidentService residentService,
            IPaymentService paymentService,
            INotificationService notificationService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _residentService = residentService;
            _paymentService = paymentService;
            _notificationService = notificationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> MobileLogin([FromBody] MobileLoginDto loginDto)
        {
            if (loginDto == null)
                return Error("بيانات الدخول مطلوبة");

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Error("البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            var token = GenerateJwtToken(user);
            var userInfo = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return Success(new MobileAuthResponse
            {
                Token = token,
                Expires = DateTime.UtcNow.AddDays(7),
                User = userInfo
            }, "تم الدخول بنجاح");
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound("المستخدم غير موجود");

            var userInfo = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return Success(userInfo);
        }

        [Authorize]
        [HttpGet("residents")]
        public async Task<IActionResult> GetResidents()
        {
            try
            {
                var residents = await _residentService.GetResidentsAsync();
                return Success(residents);
            }
            catch (Exception ex)
            {
                return Error($"فشل في جلب بيانات المقيمين: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("residents/{id}")]
        public async Task<IActionResult> GetResident(int id)
        {
            try
            {
                var resident = await _residentService.GetResidentAsync(id);
                if (resident == null)
                    return NotFound("المقيم غير موجود");

                return Success(resident);
            }
            catch (Exception ex)
            {
                return Error($"فشل في جلب بيانات المقيم: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("payments")]
        public async Task<IActionResult> CreatePayment([FromBody] MobilePaymentDto paymentDto)
        {
            if (paymentDto == null)
                return Error("بيانات الدفعة مطلوبة");

            var payment = new CreatePaymentDto
            {
                ResidentId = paymentDto.ResidentId,
                Amount = paymentDto.Amount,
                PaymentDate = paymentDto.PaymentDate,
                ForMonth = new DateTime(paymentDto.PaymentDate.Year, paymentDto.PaymentDate.Month, 1),
                PaymentMethod = paymentDto.PaymentMethod,
                Notes = paymentDto.Notes
            };

            try
            {
                var result = await _paymentService.CreatePaymentAsync(payment, UserId);
                return Success(result, "تم تسجيل الدفعة بنجاح");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("payments/history/{residentId}")]
        public async Task<IActionResult> GetPaymentHistory(int residentId)
        {
            try
            {
                var payments = await _paymentService.GetResidentPaymentHistoryAsync(residentId);
                return Success(payments);
            }
            catch (Exception ex)
            {
                return Error($"فشل في جلب سجل المدفوعات: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("maintenance")]
        public async Task<IActionResult> CreateMaintenanceRequest([FromBody] MobileMaintenanceRequest request)
        {
            if (request == null)
                return Error("بيانات طلب الصيانة مطلوبة");

            return Success(null, "تم تقديم طلب الصيانة بنجاح وسيتم معالجته قريبًا");
        }

        [Authorize]
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(UserId, unreadOnly);
                var notificationDtos = notifications.Select(n => new MobileNotificationDto
                {
               
                    Title = n.Title, 
                    Message = n.Message,
                    Date = n.CreatedAt,
                   
                    IsRead = n.IsRead, 
             
                }).ToList();

                return Success(notificationDtos);
            }
            catch (Exception ex)
            {
                return Error($"فشل في جلب الإشعارات: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id);
                return Success(null, "تم تعليم الإشعار كمقروء");
            }
            catch (Exception ex)
            {
                return Error($"فشل في تعليم الإشعار كمقروء: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            try
            {
                await _notificationService.MarkAllAsReadAsync(UserId);
                return Success(null, "تم تعليم جميع الإشعارات كمقروءة");
            }
            catch (Exception ex)
            {
                return Error($"فشل في تعليم الإشعارات كمقروءة: {ex.Message}");
            }
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}