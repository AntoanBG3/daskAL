using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models.ViewModels
{
    public class AdminApprovalsViewModel
    {
        public List<PendingUserViewModel> PendingUsers { get; set; } = new();
        public List<PendingPictureViewModel> PendingPictures { get; set; } = new();
    }

    public class PendingUserViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class PendingPictureViewModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CurrentPictureBase64 { get; set; }
        public string PendingPictureBase64 { get; set; }
    }
}
