namespace SchoolManagementSystem.Web.DTOs
{
    /// <summary>
    /// DTOs for deserializing legacy school database JSON imports.
    /// </summary>
    public class LegacySchoolDatabase
    {
        public List<LegacyStudent> Students { get; set; } = new();
        public List<LegacyTeacher> Teachers { get; set; } = new();
        public List<LegacySubject> Subjects { get; set; } = new();
    }

    public class LegacyStudent
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Class { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public Dictionary<string, List<int>> SubjectGrades { get; set; } = new();
    }

    public class LegacyTeacher
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public List<string> TeachingSubjects { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }

    public class LegacySubject
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int? TeacherId { get; set; }
        public LegacyTeacher? Teacher { get; set; }
    }
}
