using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Hryhoriichuk.University.Instagram.Web.Controllers
{
    public class MessageController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(AuthDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Display chat history with a specific user
        [Authorize]
        public async Task<IActionResult> Chat(string username)
        {
            if (username == null)
            {
                // If the username parameter is null, display a list of users to choose from
                var users = await _userManager.Users.ToListAsync();
                return View("UserList", users);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var otherUser = await _userManager.FindByNameAsync(username);
            if (otherUser == null)
            {
                return NotFound();
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == otherUser.Id) ||
                            (m.SenderId == otherUser.Id && m.ReceiverId == currentUser.Id))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            ViewData["OtherUserName"] = otherUser.UserName;
            ViewData["OtherUserProfilePicture"] = otherUser.ProfilePicturePath;

            return View(messages);
        }

        // Start a new chat with a specific user
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> NewChat(string username)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var otherUser = await _userManager.FindByNameAsync(username);
            if (otherUser == null)
            {
                return NotFound();
            }

            // Redirect to the chat page with the selected user's username
            return RedirectToAction("Chat", new { username = otherUser.UserName });
        }

        // Send a message
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverUsername, string messageContent)
        {
            var sender = await _userManager.GetUserAsync(User);
            var receiver = await _userManager.FindByNameAsync(receiverUsername);

            if (receiver != null)
            {
                var message = new Message
                {
                    SenderId = sender.Id,
                    ReceiverId = receiver.Id,
                    Content = messageContent,
                    Timestamp = DateTime.Now, // Add timestamp
                    Sender = sender, // Assign sender navigation property
                    Receiver = receiver // Assign receiver navigation property
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Redirect to the chat page with the selected user's username
                return RedirectToAction("Chat", new { username = receiverUsername });
            }

            return NotFound();
        }


    }
}