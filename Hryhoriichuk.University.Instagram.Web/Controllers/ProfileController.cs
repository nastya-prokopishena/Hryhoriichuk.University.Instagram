using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        public ProfileController(UserManager<ApplicationUser> userManager, AuthDbContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfilePictureFile)
        {
            if (ProfilePictureFile != null && ProfilePictureFile.Length > 0)
            {
                try
                {
                    // Check file extension and size
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(ProfilePictureFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Only image files are allowed.");
                        return RedirectToAction("Index", new { username = User.Identity.Name }); // Redirect to profile page
                    }

                    var maxFileSize = 5 * 1024 * 1024; // 5 MB
                    if (ProfilePictureFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Maximum file size exceeded (5 MB).");
                        return RedirectToAction("Index", new { username = User.Identity.Name }); // Redirect to profile page
                    }

                    // Generate unique file name
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ProfilePictureFile.FileName);

                    // Get the path where file will be saved
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profilepictures");
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to the uploads folder
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilePictureFile.CopyToAsync(stream);
                    }

                    // Get the ID of the currently logged-in user
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Find the user by ID
                    var user = await _userManager.FindByIdAsync(userId);

                    // Update the user's profile picture path
                    user.ProfilePicturePath = "/uploads/profilepictures/" + uniqueFileName;

                    // Save changes to the database
                    await _userManager.UpdateAsync(user);

                    return RedirectToAction("Index", new { username = User.Identity.Name }); // Redirect to profile page
                }
                catch (Exception ex)
                {
                    // Log the exception
                    ModelState.AddModelError("ProfilePictureFile", "An error occurred while uploading the file. Please try again.");
                    return RedirectToAction("Index", new { username = User.Identity.Name }); // Redirect to profile page
                }
            }
            else
            {
                ModelState.AddModelError("ProfilePictureFile", "Please select a file to upload.");
                return RedirectToAction("Index", new { username = User.Identity.Name }); // Redirect to profile page
            }
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string username)
        {

            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);

            int? unreadNotificationsCount = await _notificationService.GetUnreadNotificationsCount(currentUser.Id);

            ViewData["unreadNotificationsCount"] = unreadNotificationsCount;
            // Get the user by username
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return NotFound(); // Or handle the situation accordingly
            }

            // Get the user's posts
            var userPosts = await _context.Posts
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            var userStories = await _context.Stories
                .Where(s => s.UserId == user.Id && s.PostedAt >= DateTime.Now.AddDays(-1) && s.ExpiresAt > DateTime.Now)
                .ToListAsync();

            var followRequests = await _context.FollowRequests
                .Where(fr => fr.FollowedId == user.Id && fr.IsAccepted == false)
                .ToListAsync();

            ViewData["HasPendingFollowRequest"] = followRequests.Any(fr => fr.FollowerId == currentUser.Id);

            // Check if the currently logged-in user is following this user
            var isFollowing = false;
            if (currentUser != null)
            {
                var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == user.Id);
                isFollowing = follow != null;
            }

            // Map user information, posts, and following status to Profile model
            var model = new Profile
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Posts = userPosts, // Assign the user's posts to the model
                Stories = userStories,
                IsFollowing = isFollowing,
                ProfilePicturePath = user.ProfilePicturePath,
                IsPrivate = user.IsPrivate
            };

            // Add a flag to indicate whether the current user is viewing their own profile
            ViewData["IsCurrentUserProfile"] = currentUser != null && currentUser.Id == user.Id;

            var followersCount = await _context.Follows.CountAsync(f => f.FolloweeId == user.Id);

            // Compute followings count
            var followingsCount = await _context.Follows.CountAsync(f => f.FollowerId == user.Id);

            ViewData["FollowersCount"] = followersCount;
            ViewData["FollowingsCount"] = followingsCount;

            model.IsFollowing = isFollowing;

            return View(model);
        }


        [HttpGet]
        [Authorize]
        [Route("Search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                // If the query is empty, return an empty result
                var emptyModel = new SearchViewModel
                {
                    Query = query,
                    Users = new List<ApplicationUser>(),
                    Hashtags = new List<string>(),
                    HashtagCounts = new Dictionary<string, int>()
                };
                return PartialView("_SearchResults", emptyModel);
            }

            // Perform search logic to find users and hashtags by the query string
            var usersStartingWithQuery = await _userManager.Users
                .Where(u => u.UserName.StartsWith(query))
                .ToListAsync();

            var usersContainingQuery = await _userManager.Users
                .Where(u => u.UserName.Contains(query) && !u.UserName.StartsWith(query))
                .ToListAsync();

            var hashtag = query.StartsWith("#") ? query.Substring(1) : query;

            var posts = await _context.Posts
                .Where(p => p.Caption.Contains($"#{hashtag} ") || p.Caption.EndsWith($"#{hashtag}"))
                .ToListAsync();


            var hashtagCount = posts.Count;

            var model = new SearchViewModel
            {
                Query = query,
                Users = usersStartingWithQuery.Concat(usersContainingQuery).ToList(),
                Hashtags = new List<string> { hashtag },
                HashtagCounts = new Dictionary<string, int> { { hashtag, hashtagCount } }
            };

            return PartialView("_SearchResults", model);
        }

        [HttpGet]
        [Route("Profile/StoryView/{userId}")]
        public async Task<IActionResult> StoryView(string userId)
        {
            // Get the earliest active story for the given user ID
            var story = await _context.Stories
                .Include(s => s.User) // Include the related ApplicationUser
                .Where(s => s.UserId == userId && DateTime.Now >= s.PostedAt && DateTime.Now <= s.ExpiresAt)
                .OrderBy(s => s.PostedAt)
                .FirstOrDefaultAsync();

            if (story == null)
            {
                return NotFound(); // Return 404 if no active story is found
            }

            // Get all active stories for the given user ID
            var activeStories = await _context.Stories
                .Where(s => s.UserId == userId && DateTime.Now >= s.PostedAt && DateTime.Now <= s.ExpiresAt)
                .OrderBy(s => s.PostedAt)
                .Select(s => new ActiveStoryViewModel { Id = s.Id, PostedAt = s.PostedAt })
                .ToListAsync();

            // Get the IDs of the next and previous stories
            var nextStoryId = await _context.Stories
                .Where(s => s.UserId == story.UserId && s.Id > story.Id)
                .Select(s => s.Id)
                .OrderBy(s => s)
                .FirstOrDefaultAsync();

            var previousStoryId = await _context.Stories
                .Where(s => s.UserId == story.UserId && s.Id < story.Id)
                .Select(s => s.Id)
                .OrderByDescending(s => s)
                .FirstOrDefaultAsync();

            // Calculate the duration until the story expires
            var timeUntilExpiration = story.ExpiresAt - DateTime.Now;

            // Prepare the model data including next and previous story IDs and active stories
            var model = new StoryViewModel
            {
                UserId = story.UserId,
                User = story.User,
                ProfilePicturePath = story.User.ProfilePicturePath,
                MediaUrl = story.MediaUrl,
                PostedAt = story.PostedAt,
                ExpiresAt = timeUntilExpiration, // Calculate time remaining until expiration
                NextStoryId = nextStoryId,
                PreviousStoryId = previousStoryId,
                HasNextStory = nextStoryId != default,
                HasPreviousStory = previousStoryId != default,
                ActiveStories = activeStories
            };

            return PartialView("_StoryPartial", model);
        }

        [Route("Privacy")]
        public async Task<IActionResult> Privacy()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var privacySettings = await _context.PrivacySettings
                .FirstOrDefaultAsync(ps => ps.UserId == currentUser.Id);

            // If privacy settings don't exist, create a new one with default values
            if (privacySettings == null)
            {
                privacySettings = new PrivacySettings
                {
                    UserId = currentUser.Id,
                    IsPrivate = currentUser.IsPrivate,
                    CommentPrivacy = CommentPrivacy.Everybody  // Set default value here
                };
                _context.PrivacySettings.Add(privacySettings);
                await _context.SaveChangesAsync();  // Save the new settings
            }

            var viewModel = new PrivacySettingsViewModel
            {
                UserId = currentUser.Id, // Assuming UserId is a property of the PrivacySettingsViewModel
                IsPrivate = currentUser.IsPrivate, // Assuming IsPrivate is a property of the ApplicationUser representing the current user
                CommentPrivacy = privacySettings.CommentPrivacy
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePrivacySettings(bool isPrivate)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                // Toggle the IsPrivate property
                currentUser.IsPrivate = !currentUser.IsPrivate;
                await _context.SaveChangesAsync();
                return Json(new { success = true, isPrivate = currentUser.IsPrivate });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCommentPrivacy(int commentPrivacy)
        {
            Console.WriteLine($"Received commentPrivacy value: {commentPrivacy}");
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                // Retrieve the current user's privacy settings from the database
                var privacySettings = await _context.PrivacySettings.FirstOrDefaultAsync(ps => ps.UserId == currentUser.Id);

                // If privacy settings don't exist, create a new one
                if (privacySettings == null)
                {
                    privacySettings = new PrivacySettings
                    {
                        UserId = currentUser.Id,
                        IsPrivate = currentUser.IsPrivate,
                        CommentPrivacy = CommentPrivacy.Everybody
                    };
                    _context.PrivacySettings.Add(privacySettings);
                }

                // Update the comment privacy setting
                privacySettings.CommentPrivacy = (CommentPrivacy)commentPrivacy;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpGet]
        [Authorize]
        [Route("Profile/Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        [Route("Profile/DashboardAnalytics")]
        public async Task<IActionResult> DashboardAnalytics()
        {
            // Get the current user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Filter likes and comments by posts created by the current user
            var totalLikes = await _context.Likes
                .CountAsync(l => l.Post.UserId == userId);
            var totalComments = await _context.Comments
                .CountAsync(c => c.Post.UserId == userId);

            var topPosts = await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Likes.Count + p.Comments.Count())
                .Take(3)
                .Select(p => new {
                    PostId = p.Id,
                    p.Caption,
                    LikeCount = p.Likes.Count,
                    commentCount = p.Comments.Count,
                    UserName = p.User.UserName,
                    p.DatePosted,
                    imagePath = p.ImagePath,
                })
                .ToListAsync();

            return Json(new
            {
                totalLikes,
                totalComments,
                topPosts
            });
        }

    }

}
