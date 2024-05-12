using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            return RedirectToAction("PostDetail", new { username = user.UserName, postId = postId});
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

            return RedirectToAction("Index", new { username = currentUser.UserName });
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
                ProfilePicturePath = user.ProfilePicturePath
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

            // Extract the hashtag from the query
            // Check if the query starts with "#" and remove "#" if present
            var hashtag = query.StartsWith("#") ? query.Substring(1) : query;

            // Retrieve posts containing the exact hashtag
            var posts = await _context.Posts
                .Where(p => p.Caption.Contains($"#{hashtag} ") || p.Caption.EndsWith($"#{hashtag}"))
                .ToListAsync();


            // Get the count of posts for the hashtag
            var hashtagCount = posts.Count;

            // Combine the search results
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
        [Route("Profile/StoryView/{id}")]
        public async Task<IActionResult> StoryView(int id)
        {
            var story = await _context.Stories
                .Include(s => s.User) // Include the related ApplicationUser
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null)
            {
                return NotFound(); // Return 404 if the story is not found
            }

            // Get the IDs of the next and previous stories
            var nextStoryId = await _context.Stories
                .Where(s => s.UserId == story.UserId && s.Id > id)
                .Select(s => s.Id)
                .OrderBy(s => s)
                .FirstOrDefaultAsync();

            var previousStoryId = await _context.Stories
                .Where(s => s.UserId == story.UserId && s.Id < id)
                .Select(s => s.Id)
                .OrderByDescending(s => s)
                .FirstOrDefaultAsync();

            // Calculate the duration until the story expires
            var timeUntilExpiration = story.ExpiresAt - DateTime.Now;

            // Prepare the model data including next and previous story IDs
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
                HasPreviousStory = previousStoryId != default
            };

            return PartialView("_StoryPartial", model);
        }



    }

}
