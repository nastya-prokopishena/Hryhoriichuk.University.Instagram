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

        [HttpPost]
        public async Task<IActionResult> SendFollowRequest(string userIdToFollow)
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
                var isAlreadyRequested = await _context.FollowRequests.AnyAsync(fr => fr.FollowerId == currentUser.Id && fr.FollowedId == userIdToFollow);
                if (!isAlreadyRequested)
                {
                    var followRequest = new FollowRequest
                    {
                        FollowerId = currentUser.Id,
                        FollowedId = userIdToFollow,
                        IsAccepted = false
                    };

                    _context.FollowRequests.Add(followRequest);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Profile", new { username = userToFollow.UserName });
        }

        public async Task<IActionResult> CancelFollowRequest(string userIdToCancelRequest)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userToCancelRequest = await _userManager.FindByIdAsync(userIdToCancelRequest);

            if (currentUser == null || currentUser.Id == userIdToCancelRequest)
            {
                return NotFound(); // Handle invalid user or attempting to cancel request to oneself
            }

            var followRequest = await _context.FollowRequests.FirstOrDefaultAsync(fr => fr.FollowerId == currentUser.Id && fr.FollowedId == userIdToCancelRequest);

            if (followRequest != null)
            {
                _context.FollowRequests.Remove(followRequest);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Profile", new { username = userToCancelRequest.UserName });
        }


        [Route("FollowRequests")]
        public async Task<IActionResult> FollowRequests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound(); // Handle invalid user
            }

            // Retrieve follow requests for the current user
            var followRequests = await _context.FollowRequests
                .Include(fr => fr.Follower)
                .Where(fr => fr.FollowedId == currentUser.Id)
                .ToListAsync();

            return View(followRequests);
        }


        [HttpPost]
        public IActionResult AcceptFollowRequest(int requestId)
        {
            var followRequest = _context.FollowRequests.Find(requestId);

            if (followRequest == null)
            {
                return NotFound();
            }

            // Create a new Follow entity
            var follow = new Follow
            {
                FollowerId = followRequest.FollowerId,
                FolloweeId = followRequest.FollowedId,
                FollowDate = DateTime.Now
            };

            // Add the new follow to the database
            _context.Follows.Add(follow);

            // Remove the follow request from the database
            _context.FollowRequests.Remove(followRequest);

            // Save changes to the database
            _context.SaveChanges();

            // Redirect to the FollowRequests view or wherever you want
            return RedirectToAction("FollowRequests");
        }


        [HttpPost]
        public IActionResult RejectFollowRequest(int requestId)
        {
            var followRequest = _context.FollowRequests.Find(requestId);

            if (followRequest == null)
            {
                return NotFound();
            }

            _context.FollowRequests.Remove(followRequest);
            _context.SaveChanges();

            // Redirect to the FollowRequests view or wherever you want
            return RedirectToAction("FollowRequests");
        }


    }
}
