using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation property
        public List<Subject> TeachingSubjects { get; set; } = new();
    }
}
