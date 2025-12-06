using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagementSystem
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Class { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public Dictionary<string, List<int>> SubjectGrades { get; set; } = new();

        public string FullName => $"{FirstName} {LastName}";
        public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
        
        public double GetAverageGradeForSubject(string subject)
        {
            if (SubjectGrades.TryGetValue(subject, out var grades) && grades.Any())
                return grades.Average();
            return 0;
        }

        public double GetOverallAverageGrade()
        {
            if (!SubjectGrades.Any()) return 0;
            return SubjectGrades.Values.SelectMany(g => g).Average();
        }
    }
}
