using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class PrivacySettings
    {
        public string UserId { get; set; }

        public bool IsPrivate { get; set; }

        public CommentPrivacy CommentPrivacy { get; set; }

        public ApplicationUser User { get; set; }
    }

    public enum CommentPrivacy
    {
        Everybody,
        OnlyFollowers,
        Nobody
    }
}
