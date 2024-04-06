using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.Extensions.Hosting;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
        public DateTime LikeDate { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual Post Post { get; set; }

        public bool HasUserLiked(string userId, int postId)
        {
            return this.UserId == userId && this.PostId == postId;
        }
    }
}

