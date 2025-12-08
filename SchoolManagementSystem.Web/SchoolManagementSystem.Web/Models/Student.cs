using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagementSystem.Web.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public int? SchoolClassId { get; set; }
        public SchoolClass? SchoolClass { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Navigation property for grades
        public List<Grade> Grades { get; set; } = new();
    }
}
