using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string MediaUrl { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
