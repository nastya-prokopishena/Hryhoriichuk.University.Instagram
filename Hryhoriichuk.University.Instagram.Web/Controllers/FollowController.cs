using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class FollowController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public FollowController(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
                await _context.SaveChangesAsync();
            }
            else
            {
                var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == userIdToFollow);

                if (follow != null)
                {
                    _context.Follows.Remove(follow);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Profile", new { username = userToFollow.UserName });
        }
    }
}
