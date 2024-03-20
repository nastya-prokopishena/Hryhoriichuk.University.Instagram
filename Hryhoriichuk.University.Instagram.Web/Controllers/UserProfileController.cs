using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Check if the user is authenticated
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

            // Map user information to UserViewModel
            var userViewModel = new UserProfile
            {
                Id = currentUser.Id,
                FullName = currentUser.FullName,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
            };

            // Pass the UserViewModel to the view
            return View(userViewModel);
        }


        [HttpPost]
        public IActionResult Index(UserProfile model)
        {
            // This action can be used if you want to handle form submissions from the profile view
            return View();
        }
    }
}
