using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class SchoolClass
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public List<Student> Students { get; set; } = new();
        public List<ClassSubject> ClassSubjects { get; set; } = new();
    }
}
