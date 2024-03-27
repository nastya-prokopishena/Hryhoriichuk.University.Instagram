using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class MyProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;

        public MyProfileController(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                // If not authenticated, redirect to the login page or handle the situation accordingly
                return RedirectToAction("Login", "Account"); // Example redirect to login page
            }

            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if currentUser is null
            if (currentUser == null)
            {
                // Handle the situation where currentUser is null (e.g., log the error, return an error view)
                return View("Error"); // Example error view
            }

            // Get the ID of the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get the user's posts
            var userPosts = await _context.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync();



            // Map user information and posts to MyProfile model
            var model = new MyProfile
            {
                Id = currentUser.Id,
                FullName = currentUser.FullName,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                Posts = userPosts // Assign the user's posts to the model
            };

            return View(model);
        }
    }
}
