using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models.ViewModels
{
    public class SchoolClassViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public List<string> AssignedSubjects { get; set; } = new();
    }
}
