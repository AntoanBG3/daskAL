using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models.ViewModels
{
    public class StudentViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Class { get; set; } = string.Empty;

        [Required]
        public int? ClassId { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        
        // Flattened grades for display potentially, or kep as list of GradeViewModel
        public double AverageGrade { get; set; }
        
        // For editing/displaying
        public string FormattedDateOfBirth => DateOfBirth.ToShortDateString();
    }
}
