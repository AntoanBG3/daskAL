using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models.ViewModels
{
    public class TeacherViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        // Display purpose: List of subject names
        public List<string> TeachingSubjects { get; set; } = new();
    }
}
