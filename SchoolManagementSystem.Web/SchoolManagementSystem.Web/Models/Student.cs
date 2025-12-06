using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Class { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Navigation property for grades
        public List<Grade> Grades { get; set; } = new();
    }
}
