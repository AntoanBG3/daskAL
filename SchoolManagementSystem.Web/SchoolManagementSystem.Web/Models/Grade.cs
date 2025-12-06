using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Grade
    {
        public int Id { get; set; }

        public int Value { get; set; } // 2 to 6

        [Required]
        public string SubjectName { get; set; } = string.Empty;

        public int StudentId { get; set; }
        public Student? Student { get; set; }
    }
}
