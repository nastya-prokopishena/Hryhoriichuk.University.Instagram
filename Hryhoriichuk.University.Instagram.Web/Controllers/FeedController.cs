using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class FeedController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public FeedController(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> FollowUser(string userIdToFollow)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userToFollow = await _userManager.FindByIdAsync(userIdToFollow);

            if (currentUser != null && userToFollow != null)
            {
                // Check if the current user is already following the userToFollow
                var alreadyFollowing = currentUser.Followings.Any(f => f.FolloweeId == userToFollow.Id);
                if (!alreadyFollowing)
                {
                    var follow = new Follow { FollowerId = currentUser.Id, FolloweeId = userToFollow.Id };
                    _context.Follows.Add(follow);
                    await _context.SaveChangesAsync();
                    return Ok("User followed successfully.");
                }
                return BadRequest("User is already followed.");
            }
            return BadRequest("User or follower not found.");
        }

        [HttpGet]
        public async Task<IActionResult> Feed()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                return View();
            }
            return BadRequest("User not found.");
        }
    }
}
