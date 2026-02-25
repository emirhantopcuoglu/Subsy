namespace Subsy.Web.Models
{
    public class ProfileViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; }
        public string ProfilePhotoPath { get; set; } = "/images/user-placeholder.png";

        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
