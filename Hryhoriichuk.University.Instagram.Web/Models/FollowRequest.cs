using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class FollowRequest
    {
        public int Id { get; set; }
        public string FollowerId { get; set; } // Id of the user sending the follow request
        public ApplicationUser Follower { get; set; }
        public string FollowedId { get; set; } // Id of the user receiving the follow request
        public bool IsAccepted { get; set; } // Indicates whether the follow request is accepted or not
    }

}
