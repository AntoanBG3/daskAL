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

                        // 2. Import Subjects and Link Teachers
                        if (!_context.Subjects.Any() && schoolData.Subjects != null)
                        {
                             var teachers = await _context.Teachers.ToListAsync();

                             foreach(var s in schoolData.Subjects)
                             {
                                 // Try to find teacher by name if ID is not reliable or matching legacy structure
                                 // LegacySubject has Teacher object or TeacherId?
                                 // Let's assume LegacySubject has Teacher Name or we match by TeachingSubjects from Teacher list

                                 // Logic: Find a teacher who teaches this subject
                                 var teacher = teachers.FirstOrDefault(t =>
                                     schoolData.Teachers != null && schoolData.Teachers.Any(lt => lt.FirstName == t.FirstName && lt.LastName == t.LastName && lt.TeachingSubjects.Contains(s.Name))
                                 );

                                 _context.Subjects.Add(new Subject
                                 {
                                     Name = s.Name,
                                     Description = s.Description,
                                     TeacherId = teacher?.Id
                                 });
                             }
                             await _context.SaveChangesAsync();
                        }
                        
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
                                    // Fetch subjects to link IDs
                                    var subjects = await _context.Subjects.ToListAsync();

                                    foreach (var subjectName in s.SubjectGrades.Keys)
                                    {
                                        var subjectEntity = subjects.FirstOrDefault(sub => sub.Name == subjectName);

                                        foreach (var val in s.SubjectGrades[subjectName])
                                        {
                                            newStudent.Grades.Add(new Grade 
                                            { 
                                                SubjectName = subjectName, 
                                                SubjectId = subjectEntity?.Id,
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
