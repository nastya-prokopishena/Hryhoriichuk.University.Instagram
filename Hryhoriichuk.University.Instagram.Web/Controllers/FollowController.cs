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
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdToFollow);

            if (currentUser != null && userToFollow != null)
            {
                if (!currentUser.Followings.Any(f => f.FolloweeId == userIdToFollow))
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
            }

            return RedirectToAction("Index", "Profile", new { username = userToFollow.UserName }); // Redirect to appropriate page
        }

        // Action to unfollow a user
        [HttpPost]
        public async Task<IActionResult> Unfollow(string userIdToUnfollow)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (currentUser != null)
            {
                var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == userIdToUnfollow);

                if (follow != null)
                {
                    _context.Follows.Remove(follow);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "Profile", new { username = currentUser.UserName }); // Redirect to appropriate page
        }
    }
}
