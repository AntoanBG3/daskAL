using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SchoolManagementSystem.Web.Services
{
    public class DataImportService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<DataImportService> _logger;

        public DataImportService(SchoolDbContext context, ILogger<DataImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ImportFromLegacyJsonAsync(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(jsonPath);
                    await ImportFromJsonAsync(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading or parsing file: {JsonPath}", jsonPath);
                }
            }
            else
            {
                _logger.LogWarning("Import file not found: {JsonPath}", jsonPath);
            }
        }

        public async Task ImportFromJsonAsync(string jsonContent)
        {
            try
            {
                var schoolData = JsonSerializer.Deserialize<LegacySchoolDatabase>(jsonContent);

                if (schoolData != null)
                {
                    // 1. Import Teachers
                    if (!await _context.Teachers.AnyAsync() && schoolData.Teachers != null)
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
                        _logger.LogInformation("Imported {Count} teachers.", schoolData.Teachers.Count);
                    }

                    // 2. Import Subjects and Link Teachers
                    if (!await _context.Subjects.AnyAsync() && schoolData.Subjects != null)
                    {
                         var teachers = await _context.Teachers.ToListAsync();

                         foreach(var s in schoolData.Subjects)
                         {
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
                         _logger.LogInformation("Imported {Count} subjects.", schoolData.Subjects.Count);
                    }

                    // 3. Import Classes (New Logic)
                    if (!await _context.SchoolClasses.AnyAsync() && schoolData.Students != null)
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
                        _logger.LogInformation("Imported {Count} classes.", legacyClasses.Count);
                    }

                    // 4. Import Students and Assign to Classes
                    if (!await _context.Students.AnyAsync() && schoolData.Students != null)
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
                        _logger.LogInformation("Imported {Count} students.", schoolData.Students.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing imported data.");
                throw; // Re-throw to caller or handle? Since this is now a public helper, maybe throw.
            }
        }
    }

    // Helper classes for JSON deserialization
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
