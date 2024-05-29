using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string NotificationType { get; set; }
        public string UserIdTriggered { get; set; }
        public string UserIdReceived { get; set; }
        public int? PostId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
        public DateTime ReadTimestamp { get; set; }
        public string Message { get; set; }

        public ApplicationUser UserTriggered { get; set; } // User who triggered the action
        public ApplicationUser UserReceived { get; set; } // User who received the notification
        public Post Post { get; set; }
    }

}
