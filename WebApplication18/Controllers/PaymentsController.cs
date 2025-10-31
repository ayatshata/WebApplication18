using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MughtaribatHouse.Hubs;
using MughtaribatHouse.Services;

namespace MughtaribatHouse.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PaymentsController(IPaymentService paymentService, IHubContext<NotificationHub> hubContext)
        {
            _paymentService = paymentService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var payments = await _paymentService.GetPaymentsAsync(fromDate, toDate);
            return Success(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentAsync(id);
            if (payment == null)
                return NotFound("الدفعة غير موجودة");

            return Success(payment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            if (!ModelState.IsValid)
                return Error("بيانات الدفعة غير صحيحة", ModelState);

            try
            {
                var payment = await _paymentService.CreatePaymentAsync(dto, UserId);

                // ✅ إشعار لحظي عبر SignalR عند إنشاء دفعة جديدة
                await _hubContext.Clients.All.SendAsync(
                    "ReceiveNotification",
                    $"💰 تم تسجيل دفعة جديدة بمبلغ {dto.Amount} للساكن رقم {dto.ResidentId}."
                );

                return Success(payment, "تم تسجيل الدفعة بنجاح");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (!result)
                return NotFound("الدفعة غير موجودة");

            // ✅ إشعار عند الحذف مع اسم المستخدم
            var username = User.Identity?.Name ?? "مستخدم غير معروف";
            await _hubContext.Clients.All.SendAsync(
                "ReceiveNotification",
                $"🗑️ تم حذف دفعة رقم {id} من قبل {username}"
            );

            return Success(null, "تم حذف الدفعة بنجاح");
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            var pendingPayments = await _paymentService.GetPendingPaymentsAsync();
            return Success(pendingPayments);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetTotalRevenue([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var revenue = await _paymentService.GetTotalRevenueAsync(fromDate, toDate);
            return Success(new { revenue });
        }
    }
}
