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
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(UserManager<ApplicationUser> userManager, AuthDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        [Route("Profile/{username}/{postId}")]
        public async Task<IActionResult> PostDetail(string username, int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound();
            }

            // Retrieve additional information like likes and comments
            var currentUser = await _userManager.GetUserAsync(User);
            var isLiked = await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == currentUser.Id);
            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
            var likers = await _context.Likes
                .Include(l => l.User) // Include the User navigation property
                .Where(l => l.PostId == postId)
                .Select(l => l.User.UserName) // Select only the usernames
                .ToListAsync();

            ViewBag.likeCount = likeCount;
            ViewBag.isLiked = isLiked;

            var postUser = await _userManager.FindByIdAsync(post.UserId);
            var postUserProfilePicturePath = postUser.ProfilePicturePath;

            var viewModel = new PostInfo
            {
                Post = post,
                IsLiked = isLiked,
                LikeCount = likeCount,
                Comments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.PostId == postId)
                    .ToListAsync(),
                CurrentUserLiked = isLiked,
                ProfilePicturePath = postUserProfilePicturePath,
                UserManager = _userManager
            };


            ViewBag.Likers = likers;
            ViewBag.PostUsername = username;
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [Route("Post/AddComment")]
        public async Task<IActionResult> AddComment(int postId, string commentText)
        {
            // Retrieve the post by postId
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            // Create a new comment
            var comment = new Comment
            {
                PostId = postId,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value, // Get the user's ID from claims
                Text = commentText,
                CommentDate = DateTime.Now // You might want to adjust this according to your requirements
            };

            // Add the comment to the database
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Redirect back to the post detail page
            var user = await _userManager.FindByIdAsync(post.UserId); // Retrieve the user associated with the post
            return RedirectToAction("PostDetail", new { username = user.UserName, postId = postId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            // Retrieve the post by postId
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            // Get the currently logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the user has already liked the post
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == currentUser.Id);

            if (existingLike != null)
            {
                // User has already liked the post, so unlike it
                _context.Likes.Remove(existingLike);
            }
            else
            {
                // User has not liked the post yet, so add a like
                var like = new Like
                {
                    PostId = postId,
                    UserId = currentUser.Id,
                    LikeDate = DateTime.Now, // You might want to adjust this according to your requirements
                };
                _context.Likes.Add(like);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to the post detail page
            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            var user = await _userManager.FindByIdAsync(post.UserId); // Retrieve the user associated with the post
            return RedirectToAction("PostDetail", new { username = user.UserName, postId = postId});
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index(string username)
        {
            // Get the current user
            var currentUser = await _userManager.GetUserAsync(User);

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
                IsFollowing = isFollowing,
                ProfilePicturePath = user.ProfilePicturePath
            };

            // Add a flag to indicate whether the current user is viewing their own profile
            ViewData["IsCurrentUserProfile"] = currentUser != null && currentUser.Id == user.Id;

            model.IsFollowing = isFollowing;

            return View(model);
        }

        [HttpGet]
        [Authorize]
        [Route("Explore")]
        public async Task<IActionResult> Explore()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Get all posts
            var allPosts = await _context.Posts.Include(p => p.User).ToListAsync();

            // Define weights for likes and comments
            const double likeWeight = 1.0;
            const double commentWeight = 2.0; // Assuming comments are more engaging than likes

            // Calculate followerScore for each post
            var postsWithScores = allPosts.Select(post =>
            {
                var likeCount = _context.Likes.Count(l => l.PostId == post.Id);
                var commentCount = _context.Comments.Count(c => c.PostId == post.Id);
                var interactions = (likeWeight * likeCount) + (commentWeight * commentCount);
                var followerCount = _context.Follows.Count(f => f.FolloweeId == post.User.Id);
                var followerScore = followerCount > 0 ? interactions * followerCount : 1;

                return new
                {
                    Post = post,
                    FollowerScore = followerScore
                };
            });

            // Sort posts by followerScore in descending order
            var sortedPosts = postsWithScores.OrderByDescending(p => p.FollowerScore).Select(p => p.Post);

            return View(sortedPosts);
        }

    }

}
