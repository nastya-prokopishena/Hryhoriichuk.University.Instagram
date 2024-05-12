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

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class PostController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(AuthDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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



    }
}
