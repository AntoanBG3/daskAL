using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Absence
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int? SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public bool IsExcused { get; set; } = false;
        
        // Optional: Reason
        public string? Reason { get; set; }
    }
}
