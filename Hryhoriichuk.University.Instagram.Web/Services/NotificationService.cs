using System;
using System.Threading.Tasks;
using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hryhoriichuk.University.Instagram.Web.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string notificationType, string userIdTriggered, string userIdReceived, int? postId);
        Task CreateNotificationNoPost(string notificationType, string userIdTriggered, string userIdReceived);
        Task DeleteNotificationAsync(int notificationId);
        Task<int> GetUnreadNotificationsCount(string userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AuthDbContext _context;

        public NotificationService(AuthDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string notificationType, string userIdTriggered, string userIdReceived, int? postId)
        {
            var notification = new Notification
            {
                NotificationType = notificationType,
                UserIdTriggered = userIdTriggered,
                UserIdReceived = userIdReceived,
                PostId = postId,
                Timestamp = DateTime.Now,
                IsRead = false
            };

            notification.Message = GetMessageForNotification(notification);

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task CreateNotificationNoPost(string notificationType, string userIdTriggered, string userIdReceived)
        {
            var notification = new Notification
            {
                NotificationType = notificationType,
                UserIdTriggered = userIdTriggered,
                UserIdReceived = userIdReceived,
                PostId = null, // Set PostId to null when creating a notification without a post
                Timestamp = DateTime.Now,
                IsRead = false
            };

            notification.Message = GetMessageForNotification(notification);

            _context.Notifications.Add(notification);
        }

        public async Task DeleteNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }
        }

        private string GetMessageForNotification(Notification notification)
        {
            switch (notification.NotificationType)
            {
                case "Like":
                    return $" liked your post.";
                case "Comment":
                    return $" commented on your post.";
                case "Follow":
                    return $" started following you.";
                default:
                    return "Unknown notification type";
            }
        }

        public async Task<int> GetUnreadNotificationsCount(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserIdReceived == userId && !n.IsRead)
                .CountAsync();
        }

    }
}