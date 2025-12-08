using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models.ViewModels
{
    public class GradeViewModel
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please select a subject")]
        public int? SubjectId { get; set; } // For selection
        
        public string SubjectName { get; set; } = string.Empty; // For display

        [Range(2, 6, ErrorMessage = "Grade must be between 2 and 6")]
        public int Value { get; set; }
    }
}
