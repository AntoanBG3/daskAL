using System.Collections.Generic;

namespace SchoolManagementSystem
{
    public class SchoolDatabase
    {
        public List<Student> Students { get; set; } = new();
        public List<Teacher> Teachers { get; set; } = new();
        public List<Subject> Subjects { get; set; } = new();
    }
}
