using Microsoft.Extensions.Hosting;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }

        // Navigation property for related post
        // public Post Post { get; set; }
    }
}

