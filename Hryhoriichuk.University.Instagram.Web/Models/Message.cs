using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        // Reference to the sender of the message
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        // Reference to the receiver of the message
        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}
