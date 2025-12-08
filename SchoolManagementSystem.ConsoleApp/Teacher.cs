using System;
using System.Collections.Generic;
using System.Linq;

namespace SchoolManagementSystem
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public List<string> TeachingSubjects { get; set; } = new();

        public string FullName => $"{FirstName} {LastName}";
        
        public bool TeachesSubject(string subject)
        {
            return TeachingSubjects.Any(s => s.Equals(subject, StringComparison.OrdinalIgnoreCase));
        }
    }
}
