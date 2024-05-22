using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hryhoriichuk.University.Instagram.Web.Services;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly INotificationService _notificationService;

        public PostController(UserManager<ApplicationUser> userManager, AuthDbContext context, IWebHostEnvironment webHostEnvironment, INotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult UploadMedia()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadMedia(Post post, IFormFile file, string croppedImageData)
        {
            if (file != null && file.Length > 0)
            {
                // Check file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".mp4", ".avi", ".mkv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("File", "Only image and video files are allowed.");
                    return View(post);
                }

                // Check file size
                var maxFileSize = 10 * 1024 * 1024; // 10 MB
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", "Maximum file size exceeded (10 MB).");
                    return View(post);
                }

                // Generate unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);

                // Get the path where file will be saved
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file to the uploads folder
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(croppedImageData))
                {
                    // Convert the base64 string to a byte array
                    byte[] bytes = Convert.FromBase64String(croppedImageData);
                    using (var stream = new MemoryStream(bytes))
                    {
                        // Save the cropped image to the uploads folder
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                    }
                }


                // Set the image path for the post
                post.ImagePath = "/uploads/" + uniqueFileName;

                // Set other properties of the post
                post.DatePosted = DateTime.Now;

                // Get the ID of the currently logged-in user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                post.UserId = userId;

                // Save the post to the database
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home"); // Redirect to homepage or profile page
            }
            else
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
                return View(post);
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

            ViewData["IsCurrentUserProfile"] = currentUser != null && currentUser.Id == user.Id;


            var isFollowing = false;
            if (currentUser != null)
            {
                var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FolloweeId == user.Id);
                isFollowing = follow != null;
            }

            var userProfile = await _context.Users.FirstOrDefaultAsync(p => p.UserName == username);

            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsPrivate = userProfile?.IsPrivate ?? false;

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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var privacySettings = await _context.PrivacySettings.FirstOrDefaultAsync(ps => ps.UserId == post.UserId);
            if (privacySettings == null || privacySettings.CommentPrivacy == CommentPrivacy.Nobody)
            {
                return BadRequest("Commenting is not allowed on this post.");
            }

            // Create a new comment
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId, // Get the user's ID from claims
                Text = commentText,
                CommentDate = DateTime.Now // You might want to adjust this according to your requirements
            };

            // Add the comment to the database
            _context.Comments.Add(comment);
            await _notificationService.CreateNotificationAsync("Comment", userId, post.UserId, postId);
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

                var existingNotification = await _context.Notifications.FirstOrDefaultAsync(n =>
                    n.NotificationType == "Like" &&
                    n.UserIdTriggered == currentUser.Id &&
                    n.PostId == postId);

                if (existingNotification != null)
                {
                    _context.Notifications.Remove(existingNotification);
                }
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
                await _notificationService.CreateNotificationAsync("Like", currentUser.Id, post.UserId, postId);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect back to the post detail page
            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            var user = await _userManager.FindByIdAsync(post.UserId); // Retrieve the user associated with the post
            return RedirectToAction("PostDetail", new { username = user.UserName, postId = postId });
        }

        [HttpPost]
        [Authorize]
        [Route("Profile/DeletePost")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization
            var currentUser = await _userManager.GetUserAsync(User);
            if (post.UserId != currentUser.Id)
            {
                return Forbid(); // User is not authorized to delete this post
            }

            // Delete related likes, comments, and notifications
            _context.Likes.RemoveRange(_context.Likes.Where(l => l.PostId == postId));
            _context.Comments.RemoveRange(_context.Comments.Where(c => c.PostId == postId));
            _context.Notifications.RemoveRange(_context.Notifications.Where(n => n.PostId == postId));

            // Delete the post
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Profile", new { username = currentUser.UserName });
        }

        [HttpPost]
        [Authorize]
        [Route("Profile/EditCaption")]
        public async Task<IActionResult> EditCaption(int postId, string captionText)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization
            var currentUser = await _userManager.GetUserAsync(User);
            if (post.UserId != currentUser.Id)
            {
                return Forbid(); // User is not authorized to edit this post
            }

            // Update the caption
            post.Caption = captionText;
            await _context.SaveChangesAsync();

            return RedirectToAction("PostDetail", new { username = post.User.UserName, postId = postId });
        }

        [HttpGet]
        [Authorize]
        [Route("Explore/Hashtag/{hashtag}")]
        public async Task<IActionResult> PostsByHashtag(string hashtag)
        {
            // Retrieve posts containing the hashtag
            var posts = await _context.Posts
                .Where(p => p.Caption.Contains(hashtag))
                .Include(p => p.User) // Include the User navigation property to access user details
                .ToListAsync();

            // Convert posts to PostInfo objects
            var postInfos = posts.Select(post => new PostInfo
            {
                Post = post,
                User = post.User,

                // You may need to populate other properties like IsLiked, LikeCount, Likers, Comments, etc.
            }).ToList();

            // Pass the hashtag and postInfos to the view
            ViewBag.Hashtag = hashtag;

            return View(postInfos);
        }


        [HttpGet]
        [Authorize]
        public IActionResult UploadStory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadStory(Story story, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".mp4", ".avi", ".mkv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("File", "Only image and video files are allowed.");
                    return View();
                }

                // Check file size
                var maxFileSize = 10 * 1024 * 1024; // 10 MB
                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError("File", "Maximum file size exceeded (10 MB).");
                    return View();
                }

                // Generate unique file name
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);

                // Get the path where file will be saved
                var storiesFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "stories");
                var filePath = Path.Combine(storiesFolder, uniqueFileName);

                // Save the file to the uploads folder
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Set the media URL for the story
                story.MediaUrl = "/uploads/stories/" + uniqueFileName;

                // Set other properties of the story
                story.PostedAt = DateTime.Now;
                story.ExpiresAt = DateTime.Now.AddDays(1); // Example: Story expires after 1 day
                story.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Save the story to the database
                _context.Stories.Add(story);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home"); // Redirect to homepage or profile page
            }
            else
            {
                ModelState.AddModelError("File", "Please select a file to upload.");
                return View(story);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("Explore")]
        public async Task<IActionResult> Explore()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            // Get IDs of users whom the current user is following
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FolloweeId)
                .ToListAsync();

            // Get IDs of users who have private accounts
            var privateUserIds = await _context.Users
                .Where(u => u.IsPrivate)
                .Select(u => u.Id)
                .ToListAsync();

            // Get all posts from users whom the current user is following or who don't have private accounts
            var filteredPosts = await _context.Posts
                .Include(p => p.User)
                .Where(p => !privateUserIds.Contains(p.UserId) || followingIds.Contains(p.UserId))
                .ToListAsync();

            const double likeWeight = 1.0;
            const double commentWeight = 2.0;

            // Calculate followerScore for filtered posts
            var postsWithScores = filteredPosts.Select(post =>
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

            // Sort filtered posts by followerScore in descending order
            var sortedPosts = postsWithScores.OrderByDescending(p => p.FollowerScore).Select(p => p.Post);

            return View(sortedPosts);
        }

        [HttpGet]
        [Authorize]
        [Route("PostInsights")]
        public async Task<IActionResult> PostInsights(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
            var commentCount = await _context.Comments.CountAsync(c => c.PostId == postId);

            var viewModel = new PostInsightsViewModel
            {
                PostId = postId,
                LikeCount = likeCount,
                CommentCount = commentCount,
                User = currentUser,
                Post = post
            };

            return PartialView("_PostInsightsPartial", viewModel);
        }
    }
}
