using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Grade
    {
        public int Id { get; set; }

        [Range(2, 6, ErrorMessage = "Grade must be between 2 and 6")]
        public int Value { get; set; } // 2 to 6

        public int? SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}
