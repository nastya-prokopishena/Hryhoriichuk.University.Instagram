using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
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

        public ProfileController(UserManager<ApplicationUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

            var viewModel = new PostInfo
            {
                Post = post,
                IsLiked = isLiked,
                LikeCount = likeCount,
                Comments = await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.PostId == postId)
                    .ToListAsync(),
                CurrentUserLiked = isLiked
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
            var currentUser = await _userManager.GetUserAsync(User);
            var isFollowing = currentUser != null && currentUser.Followings != null && currentUser.Followings.Any(f => f.FolloweeId == user.Id);

            // Map user information, posts, and following status to Profile model
            var model = new Profile
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                Posts = userPosts, // Assign the user's posts to the model
                IsFollowing = isFollowing
            };

            return View(model);
        }
    }

}
