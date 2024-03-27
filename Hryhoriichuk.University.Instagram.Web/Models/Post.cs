using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
namespace Hryhoriichuk.University.Instagram.Web.Models
{
    [Table("Posts")]
    public class Post
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ImagePath { get; set; }
        public string Caption { get; set; }
        public DateTime DatePosted { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public IFormFile ImageFile { get; set; } // For file upload

        // Navigation properties
        public virtual ICollection<Comment> Comments { get; set; }
        // public virtual ICollection<UserImageLike> Likes { get; set; }
        // public virtual ICollection<UserImageDislike> Dislikes { get; set; }

        public Post()
        {
            Comments = new HashSet<Comment>();
            // Likes = new HashSet<UserImageLike>();
            // Dislikes = new HashSet<UserImageDislike>();
        }
    }
}
