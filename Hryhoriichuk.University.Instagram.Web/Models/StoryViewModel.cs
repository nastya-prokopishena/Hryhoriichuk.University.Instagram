using Hryhoriichuk.University.Instagram.Web.Areas.Identity.Data;

namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class StoryViewModel
    {
        public string UserId { get; set; } // ID of the user who posted the story
        public virtual ApplicationUser User { get; set; }
        public string ProfilePicturePath { get; set; } // Profile picture of the user who posted the story
        public string MediaUrl { get; set; } // URL of the story media
        public DateTime PostedAt { get; set; } // Date and time when the story was posted
        public TimeSpan ExpiresAt { get; set; } // Duration until the story expires
        public int NextStoryId { get; set; } // ID of the next story of the same user
        public int PreviousStoryId { get; set; } // ID of the previous story of the same user
        public bool HasNextStory { get; set; } // Indicates if there is a next story of the same user
        public bool HasPreviousStory { get; set; } // Indicates if there is a previous story of the same user

        public List<ActiveStoryViewModel> ActiveStories { get; set; } // List of active stories

        public StoryViewModel()
        {
            ActiveStories = new List<ActiveStoryViewModel>();
        }
    }

    public class ActiveStoryViewModel
    {
        public int Id { get; set; } // ID of the active story
        public DateTime PostedAt { get; set; } // Date and time when the story was posted
    }
}
