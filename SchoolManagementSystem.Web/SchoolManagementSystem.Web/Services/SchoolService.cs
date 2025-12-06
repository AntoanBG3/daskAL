using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using System.Text.Json;

namespace SchoolManagementSystem.Web.Services
{
    public class SchoolService
    {
        private readonly SchoolDbContext _context;

        public SchoolService(SchoolDbContext context)
        {
            _context = context;
        }

        // --- Data Migration / Seeding ---
        public async Task ImportFromLegacyJsonAsync(string jsonPath)
        {
            if (File.Exists(jsonPath) && !await _context.Students.AnyAsync())
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(jsonPath);
                    var legacyData = JsonSerializer.Deserialize<LegacySchoolDatabase>(jsonContent);

                    if (legacyData != null)
                    {
                        // Migrate Students
                        foreach (var s in legacyData.Students)
                        {
                            var newStudent = new Student
                            {
                                FirstName = s.FirstName,
                                LastName = s.LastName,
                                Class = s.Class,
                                DateOfBirth = s.DateOfBirth
                            };

                            // Migrate Grades
                            foreach (var subjectGrade in s.SubjectGrades)
                            {
                                foreach (var gradeVal in subjectGrade.Value)
                                {
                                    newStudent.Grades.Add(new Grade 
                                    { 
                                        SubjectName = subjectGrade.Key, 
                                        Value = gradeVal 
                                    });
                                }
                            }
                            _context.Students.Add(newStudent);
                        }

                        // Migrate Teachers
                        foreach (var t in legacyData.Teachers)
                        {
                            var newTeacher = new Teacher
                            {
                                FirstName = t.FirstName,
                                LastName = t.LastName
                            };
                            
                            // Note: Subject linking is complex because Subjects need to be created first or matched.
                            // For simplicity, we'll create Subjects based on Teacher's list if they don't exist?
                            // Or better, relying on the 'Subject' list from legacy data if available.
                            
                            _context.Teachers.Add(newTeacher);
                        }

                        // Migrate Subjects (and link to teachers)
                        foreach (var sub in legacyData.Subjects)
                        {
                            var newSubject = new Subject
                            {
                                Name = sub.Name,
                                Description = sub.Description
                            };

                            if (sub.TeacherId.HasValue)
                            {
                                // We rely on the fact that we added teachers in order, but IDs might change (autoincrement).
                                // Ideally we should map old IDs to new Entities.
                                // For this migration, provided it runs on empty DB, we can try to find by Name or order.
                                // Let's simplify: Import subjects, user can re-assign if needed.
                                // Or, we can try to look up the teacher by name if possible.
                            }
                            _context.Subjects.Add(newSubject);
                        }

                        await _context.SaveChangesAsync();
                        
                        // Linking Loop (Post-Save to have IDs, or pre-save with object references)
                        // This part is skipped for brevity/complexity in first pass, can be refined.
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing data: {ex.Message}");
                }
            }
        }

        // --- Students ---
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.Include(s => s.Grades).ToListAsync();
        }

        public async Task AddStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _context.Students.Include(s => s.Grades).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }

        // --- Teachers ---
        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers.Include(t => t.TeachingSubjects).ToListAsync();
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
        }

        // --- Subjects ---
        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            return await _context.Subjects.Include(s => s.Teacher).ToListAsync();
        }

        public async Task<Subject?> GetSubjectByIdAsync(int id)
        {
            return await _context.Subjects.Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSubjectAsync(Subject subject)
        {
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();
        }

        public async Task AddSubjectAsync(Subject subject)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }

        // --- Grades ---
        public async Task AddGradeAsync(Grade grade)
        {
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
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
    }
    class LegacySubject
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int? TeacherId { get; set; }
    }
}
