﻿namespace Hryhoriichuk.University.Instagram.Web.Models
{
    public class PrivacySettingsViewModel
    {
        public string UserId { get; set; }
        public bool IsPrivate { get; set; }
        public CommentPrivacy CommentPrivacy { get; set; }
    }
}
