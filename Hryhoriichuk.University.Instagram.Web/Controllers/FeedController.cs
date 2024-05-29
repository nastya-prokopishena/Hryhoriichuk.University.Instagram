using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Feed()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Retrieve the list of users the current user follows
            var followedUsers = await _context.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FolloweeId)
                .ToListAsync();

            // Retrieve posts from followed users
            var feedPosts = await _context.Posts
                .Where(p => followedUsers.Contains(p.UserId))
                .Include(p => p.User) // Include the user who made the post
                .OrderByDescending(p => p.DatePosted) // Order by date posted (newest first)
                .ToListAsync();

            return View(feedPosts);
        }
    }
}
