using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hryhoriichuk.University.Instagram.Core.Interfaces;
using Hryhoriichuk.University.Instagram.Models.Configuration;
using Microsoft.EntityFrameworkCore;
using Hryhoriichuk.University.Instagram.Web.Data;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Services;
using Assert = Xunit.Assert;

namespace Hryhoriichuk.University.Instagram.Test
{
    public class NotificationServiceTests : TestBase
    {
        private readonly ILogger<NotificationServiceTests> _logger;
        private readonly IOptions<AppConfig> _configuration;

        public NotificationServiceTests()
        {
            _logger = ResolveService<ILogger<NotificationServiceTests>>();
            _configuration = ResolveService<IOptions<AppConfig>>();
        }

        [Fact]
        public async Task CreateNotificationAsync_Success()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Db")
                .Options;

            using (var context = new AuthDbContext(options))
            {
                var notificationService = new NotificationService(context);

                // Act
                await notificationService.CreateNotificationAsync("Like", "user1", "user2", 1);

                // Assert
                var notification = await context.Notifications.FirstOrDefaultAsync();
                Assert.NotNull(notification);
                Assert.Equal("Like", notification.NotificationType);
                Assert.Equal("user1", notification.UserIdTriggered);
                Assert.Equal("user2", notification.UserIdReceived);
                Assert.Equal(1, notification.PostId);
            }
        }

        [Fact]
        public async Task DeleteNotificationAsync_Success()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_Db")
                .Options;

            using (var context = new AuthDbContext(options))
            {
                var notification = new Notification
                {
                    Id = 1,
                    NotificationType = "Like",
                    UserIdTriggered = "user1",
                    UserIdReceived = "user2",
                    PostId = 1
                };

                context.Notifications.Add(notification);
                context.SaveChanges();

                var notificationService = new NotificationService(context);

                // Act
                await notificationService.DeleteNotificationAsync(1);

                // Assert
                var deletedNotification = await context.Notifications.FindAsync(1);
                Assert.Null(deletedNotification);
            }
        }
    }
}