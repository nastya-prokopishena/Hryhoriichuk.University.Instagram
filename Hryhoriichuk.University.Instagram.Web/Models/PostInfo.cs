using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class PostInfo
    {
        public Post Post { get; set; }
        public bool IsLiked { get; set; }
        public int LikeCount { get; set; }
        public virtual ApplicationUser User { get; set; }
        public List<ApplicationUser> Likers { get; set; }
        public List<Comment> Comments { get; set; }

        public string ProfilePicturePath { get; set; }

        public UserManager<ApplicationUser> UserManager { get; set; }
        public string ImagePath => Post.ImagePath;
        public string? Caption => Post.Caption;
        public DateTime DatePosted => Post.DatePosted;
        public int Id => Post.Id;
        public bool CurrentUserLiked { get; set; }
    }
}
