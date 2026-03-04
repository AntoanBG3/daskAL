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

        public string Class { get; set; } = string.Empty;

        [Required]
        public int? ClassId { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
        
        // Flattened grades for display potentially, or kep as list of GradeViewModel
        public double AverageGrade { get; set; }
        
        // For editing/displaying
        public string FormattedDateOfBirth => DateOfBirth.ToShortDateString();
    }
}
