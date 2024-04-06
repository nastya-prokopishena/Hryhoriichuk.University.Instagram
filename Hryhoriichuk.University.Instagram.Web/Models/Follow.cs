using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Follow
    {
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        public string FolloweeId { get; set; }
        public ApplicationUser Followee { get; set; }

        // Additional properties if needed
        public DateTime? FollowDate { get; set; }
        public bool IsBlocked { get; set; }

    }
}
