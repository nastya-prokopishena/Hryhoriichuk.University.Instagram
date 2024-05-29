using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.Extensions.Hosting;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string Text { get; set; }
        public DateTime CommentDate { get; set; } // Add CommentDate property

        // Navigation properties
        public virtual Post Post { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
