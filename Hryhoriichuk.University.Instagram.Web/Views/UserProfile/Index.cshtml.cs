using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Hryhoriichuk.University.Instagram.Web.Views.UserProfile
{
    public class Index
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

        /// public List<RolesViewModel> UserInRoles { get; set; }
        public bool IsLockedOut { get; set; }
    }
}
