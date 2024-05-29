using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<Post>? Posts { get; set; }
        public List<string>? Hashtags { get; set; }
        public Dictionary<string, int>? HashtagCounts { get; set; }
    }
}
