using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class PostInsightsViewModel
    {
        public int PostId { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public ApplicationUser User { get; set; }
        public Post Post { get; set; }
    }
}