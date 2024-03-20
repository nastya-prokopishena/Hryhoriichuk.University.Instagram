using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.Extensions.Hosting;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }

        // Navigation properties for related user and post
        public ApplicationUser User { get; set; }
        // public Post Post { get; set; }
    }
}
