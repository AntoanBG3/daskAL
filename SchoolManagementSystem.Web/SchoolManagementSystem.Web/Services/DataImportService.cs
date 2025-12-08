using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using System.Text.Json;

namespace SchoolManagementSystem.Web.Services
{
    public class DataImportService
    {
        private readonly SchoolDbContext _context;

        public DataImportService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task ImportFromLegacyJsonAsync(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    var schoolData = JsonSerializer.Deserialize<LegacySchoolDatabase>(json);

                    if (schoolData != null)
                    {
                        // 1. Import Teachers
                        if (!_context.Teachers.Any() && schoolData.Teachers != null)
                        {
                            foreach (var t in schoolData.Teachers)
                            {
                                _context.Teachers.Add(new Teacher 
                                { 
                                    FirstName = t.FirstName, 
                                    LastName = t.LastName
                                    // Subjects will be linked later via name matching if needed, or manually
                                });
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 2. Import Subjects and Link Teachers (Skipped in original logic, kept commented out or similar)
                        
                        // 3. Import Classes (New Logic)
                        if (!_context.SchoolClasses.Any() && schoolData.Students != null)
                        {
                            var legacyClasses = schoolData.Students.Select(s => s.Class).Distinct().ToList();
                            foreach (var className in legacyClasses)
                            {
                                if (!string.IsNullOrWhiteSpace(className))
                                {
                                    _context.SchoolClasses.Add(new SchoolClass { Name = className });
                                }
                            }
                            await _context.SaveChangesAsync();
                        }

                        // 4. Import Students and Assign to Classes
                        if (!_context.Students.Any() && schoolData.Students != null)
                        {
                            var classes = await _context.SchoolClasses.ToListAsync();
                            
                            foreach (var s in schoolData.Students)
                            {
                                var schoolClass = classes.FirstOrDefault(c => c.Name == s.Class);
                                
                                var newStudent = new Student
                                {
                                    FirstName = s.FirstName,
                                    LastName = s.LastName,
                                    SchoolClassId = schoolClass?.Id, // Link to new Class entity
                                    DateOfBirth = s.DateOfBirth
                                };
                                
                                // Import Grades
                                if (s.SubjectGrades != null)
                                {
                                    foreach (var subjectName in s.SubjectGrades.Keys)
                                    {
                                        foreach (var val in s.SubjectGrades[subjectName])
                                        {
                                            newStudent.Grades.Add(new Grade 
                                            { 
                                                SubjectName = subjectName, 
                                                Value = val 
                                            });
                                        }
                                    }
                                }
                                
                                _context.Students.Add(newStudent);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing data: {ex.Message}");
                }
            }
        }
    }

    // Helper classes for JSON deserialization
    class LegacySchoolDatabase
    {
        public List<LegacyStudent> Students { get; set; } = new();
        public List<LegacyTeacher> Teachers { get; set; } = new();
        public List<LegacySubject> Subjects { get; set; } = new();
    }
    class LegacyStudent
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Class { get; set; } = "";
        public DateTime DateOfBirth { get; set; }
        public Dictionary<string, List<int>> SubjectGrades { get; set; } = new();
    }
    class LegacyTeacher
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public List<string> TeachingSubjects { get; set; } = new();
        public string FullName => $"{FirstName} {LastName}";
    }
    class LegacySubject
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int? TeacherId { get; set; }
        public LegacyTeacher? Teacher { get; set; }
    }
}
