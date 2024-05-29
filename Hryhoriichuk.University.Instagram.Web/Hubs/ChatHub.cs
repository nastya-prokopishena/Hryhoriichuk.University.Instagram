using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

public class ChatHub : Hub
{
    private readonly AuthDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatHub(AuthDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SendMessage(string receiverUsername, string messageContent)
    {
        var sender = await _userManager.GetUserAsync(Context.User);
        var receiver = await _userManager.FindByNameAsync(receiverUsername);
        if (receiver != null)
        {
            var message = new Message
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Content = messageContent
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Send the message to the sender and receiver
            await Clients.Users(sender.Id, receiver.Id).SendAsync("ReceiveMessage", message);
        }
    }

    public override async Task OnConnectedAsync()
    {
        // Additional logic when a client connects
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Additional logic when a client disconnects
        await base.OnDisconnectedAsync(exception);
    }
}
