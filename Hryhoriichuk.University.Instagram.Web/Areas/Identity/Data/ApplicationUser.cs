using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Hryhoriichuk.University.Instagram.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string FullName { get; set; }

    // Navigation property for users being followed by this user
    public ICollection<Follow> Followers { get; set; }

    // Navigation property for users following this user
    public ICollection<Follow> Followings { get; set; }
}