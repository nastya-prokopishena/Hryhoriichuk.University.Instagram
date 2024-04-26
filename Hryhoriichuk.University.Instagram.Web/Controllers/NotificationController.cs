using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var notifications = await _context.Notifications
                .Include(n => n.UserTriggered)  // Include the related UserTriggered entity
                .Include(n => n.Post)  // Include the related UserTriggered entity
                .Where(n => n.UserIdReceived == currentUser.Id)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Message = GetMessageForNotification(notification);
                notification.IsRead = true; // Mark as read when retrieved
            }

            await _context.SaveChangesAsync();

            return View(notifications);
        }

        private string GetMessageForNotification(Notification notification)
        {
            switch (notification.NotificationType)
            {
                case "Like":
                    return $" liked your post.";
                case "Comment":
                    return $" commented on your post.";
                case "Follow":
                    return $" started following you.";
                default:
                    return "Unknown notification type";
            }
        }
    }

}
