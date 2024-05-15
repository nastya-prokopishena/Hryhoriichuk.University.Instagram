using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class Profile
    {
        public string Id { get; set; }

        [DisplayName("Full Name")]
        public string FullName { get; set; }

        [DisplayName("User Name")]
        public string UserName { get; set; }

        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Please enter a vaild email address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DisplayName("Bio")]
        public string Bio { get; set; }
        [DisplayName("Profile Picture")]
        public string ProfilePicturePath { get; set; }
        [NotMapped] // This property won't be mapped to the database
        public IFormFile ProfilePictureFile { get; set; }

        /// public List<RolesViewModel> UserInRoles { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsFollowing { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Story> Stories { get; set; }
        public ICollection<FollowRequest> FollowRequests { get; set; }
        [DisplayName("Private Account")]
        public bool IsPrivate { get; set; }
    }
}
