using Microsoft.AspNetCore.Mvc;

namespace MughtaribatHouse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        protected string UserEmail => User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        protected IActionResult Success(object data = null, string message = null)
        {
            return Ok(new
            {
                success = true,
                data,
                message
            });
        }

        protected IActionResult Error(string message, object errors = null)
        {
            return BadRequest(new
            {
                success = false,
                message,
                errors
            });
        }

        protected IActionResult NotFound(string message = "المورد غير موجود")
        {
            return NotFound(new
            {
                success = false,
                message
            });
        }
    }
}