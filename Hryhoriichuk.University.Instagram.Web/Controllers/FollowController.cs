using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Data;
using Microsoft.AspNetCore.Identity;
using Hryhoriichuk.University.Instagram.Web.Services;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class FollowController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly INotificationService _notificationService;

        public FollowController(UserManager<ApplicationUser> userManager, AuthDbContext context, INotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _notificationService = notificationService;
        }

        // Action to follow a user
        [HttpPost]
        public async Task<IActionResult> Follow(string userIdToFollow)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userToFollow = await _userManager.FindByIdAsync(userIdToFollow);

            if (currentUser == null || userToFollow == null || currentUser.Id == userIdToFollow)
            {
                return NotFound(); // Handle invalid user or attempting to follow oneself
            }

            var isAlreadyFollowing = await _context.Follows.AnyAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == userIdToFollow);
            if (!isAlreadyFollowing)
            {
                var follow = new Follow
                {
                    FollowerId = currentUser.Id,
                    FolloweeId = userIdToFollow,
                    FollowDate = DateTime.Now
                };

                _context.Follows.Add(follow);
                await _notificationService.CreateNotificationNoPost("Follow", currentUser.Id, userToFollow.Id);

                await _context.SaveChangesAsync();
            }
            else
            {
                var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == userIdToFollow);

                if (follow != null)
                {
                    _context.Follows.Remove(follow);
                    await _context.SaveChangesAsync();

                    var existingNotifications = await _context.Notifications.Where(n =>
                        n.NotificationType == "Follow" &&
                        n.UserIdTriggered == currentUser.Id &&
                        n.UserIdReceived == userToFollow.Id).ToListAsync();

                    if (existingNotifications != null && existingNotifications.Any())
                    {
                        _context.Notifications.RemoveRange(existingNotifications);
                        await _context.SaveChangesAsync();
                    }

                }
            }

            return RedirectToAction("Index", "Profile", new { username = userToFollow.UserName });
        }
    }
}
