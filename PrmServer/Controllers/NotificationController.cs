using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrmServer.Services.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnread()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadAsync(userId);
            return Ok(notifications);
        }
    }
}
