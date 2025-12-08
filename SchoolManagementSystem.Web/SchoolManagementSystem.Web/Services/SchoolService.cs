using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;
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
                        /* 
                        // Teacher linking skipped as LegacySubject definition logic mismatch
                        var teachers = await _context.Teachers.ToListAsync();
                        foreach (var s in schoolData.Subjects)
                        {
                            // var teacher = teachers.FirstOrDefault(t => t.FirstName + " " + t.LastName == s.Teacher?.FullName); 
                            _context.Subjects.Add(new Subject
                            {
                                Name = s.Name,
                                Description = "Imported",
                                // TeacherId = teacher?.Id
                            });
                        }
                        await _context.SaveChangesAsync();
                        */
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
                                foreach (var subjectName in s.SubjectGrades.Keys)
                                {
                                    foreach (var val in s.SubjectGrades[subjectName])
                                    {
                                        newStudent.Grades.Add(new Grade 
                                        { 
                                            SubjectName = subjectName, 
                                            Value = val 
                                            // SubjectId linking would require matching names again
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

        // --- School Classes ---
        public async Task<List<SchoolClassViewModel>> GetAllClassesAsync()
        {
            var classes = await _context.SchoolClasses
                .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.Subject)
                .ToListAsync();

            return classes.Select(c => new SchoolClassViewModel
            {
                Id = c.Id,
                Name = c.Name,
                AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject.Name).ToList()
            }).ToList();
        }

        public async Task<SchoolClassViewModel?> GetClassByIdAsync(int id)
        {
            var c = await _context.SchoolClasses
                .Include(c => c.ClassSubjects)
                .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            return new SchoolClassViewModel
            {
                Id = c.Id,
                Name = c.Name,
                AssignedSubjects = c.ClassSubjects.Select(cs => cs.Subject.Name).ToList()
            };
        }

        public async Task AddClassAsync(SchoolClassViewModel model)
        {
            var schoolClass = new SchoolClass { Name = model.Name };
            _context.SchoolClasses.Add(schoolClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateClassSubjectsAsync(int classId, List<int> subjectIds)
        {
            var schoolClass = await _context.SchoolClasses
                .Include(c => c.ClassSubjects)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (schoolClass != null)
            {
                // Remove existing
                _context.ClassSubjects.RemoveRange(schoolClass.ClassSubjects);
                
                // Add new
                foreach (var subjectId in subjectIds)
                {
                    _context.ClassSubjects.Add(new ClassSubject { SchoolClassId = classId, SubjectId = subjectId });
                }
                
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task<List<SubjectViewModel>> GetAvailableSubjectsForStudentAsync(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null || student.SchoolClassId == null) return new List<SubjectViewModel>();
            
            var subjects = await _context.ClassSubjects
                .Where(cs => cs.SchoolClassId == student.SchoolClassId)
                .Include(cs => cs.Subject)
                .ThenInclude(s => s.Teacher)
                .Select(cs => cs.Subject)
                .ToListAsync();
                
            return subjects.Select(s => new SubjectViewModel
            {
                Id = s.Id,
                Name = s.Name, 
                TeacherName = s.Teacher != null ? s.Teacher.FullName : "Unassigned"
            }).ToList();
        }

        // --- Students (Updated) ---
        public async Task<List<StudentViewModel>> GetAllStudentsAsync()
        {
            var students = await _context.Students
                .Include(s => s.SchoolClass)
                .Include(s => s.Grades)
                .ToListAsync();
                
            return students.Select(s => new StudentViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Class = s.SchoolClass?.Name ?? "Unassigned", // Display Class Name
                DateOfBirth = s.DateOfBirth,
                AverageGrade = s.Grades.Any() ? s.Grades.Average(g => g.Value) : 0
            }).ToList();
        }

        public async Task AddStudentAsync(StudentViewModel model, int classId)
        {
            var student = new Student
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                SchoolClassId = classId,
                DateOfBirth = model.DateOfBirth
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }
        
        // Overload to keep compatibility if mostly needed, but prefer strict classId
        public async Task AddStudentAsync(StudentViewModel model)
        {
            if (int.TryParse(model.Class, out int classId))
            {
                await AddStudentAsync(model, classId);
            }
             // Fallback logic if needed, or handle error
        }

        public async Task<StudentViewModel?> GetStudentByIdAsync(int id)
        {
            var s = await _context.Students
                .Include(s => s.SchoolClass)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(x => x.Id == id);
                
            if (s == null) return null;

            return new StudentViewModel
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Class = s.SchoolClass?.Name ?? "Unassigned",
                DateOfBirth = s.DateOfBirth,
                AverageGrade = s.Grades.Any() ? s.Grades.Average(g => g.Value) : 0
            };
        }

        public async Task UpdateStudentAsync(StudentViewModel model, int classId)
        {
             var student = await _context.Students.FindAsync(model.Id);
            if (student != null)
            {
                student.FirstName = model.FirstName;
                student.LastName = model.LastName;
                student.SchoolClassId = classId;
                student.DateOfBirth = model.DateOfBirth;
                
                await _context.SaveChangesAsync();
            }
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
        public async Task<List<TeacherViewModel>> GetAllTeachersAsync()
        {
            var teachers = await _context.Teachers.Include(t => t.TeachingSubjects).ToListAsync();
            return teachers.Select(t => new TeacherViewModel
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                TeachingSubjects = t.TeachingSubjects.Select(s => s.Name).ToList()
            }).ToList();
        }

        public async Task<TeacherViewModel?> GetTeacherByIdAsync(int id)
        {
            var t = await _context.Teachers.Include(x => x.TeachingSubjects).FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return null;
            
            return new TeacherViewModel
            {
                Id = t.Id,
                FirstName = t.FirstName,
                LastName = t.LastName,
                TeachingSubjects = t.TeachingSubjects.Select(s => s.Name).ToList()
            };
        }

        public async Task AddTeacherAsync(TeacherViewModel model)
        {
            var teacher = new Teacher
            {
                FirstName = model.FirstName,
                LastName = model.LastName
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTeacherAsync(TeacherViewModel model)
        {
            var teacher = await _context.Teachers.FindAsync(model.Id);
            if (teacher != null)
            {
                teacher.FirstName = model.FirstName;
                teacher.LastName = model.LastName;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
        }

        // --- Subjects ---
        public async Task<List<SubjectViewModel>> GetAllSubjectsAsync()
        {
            var subjects = await _context.Subjects.Include(s => s.Teacher).ToListAsync();
            return subjects.Select(s => new SubjectViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher != null ? s.Teacher.FullName : "Unassigned"
            }).ToList();
        }

        public async Task<SubjectViewModel?> GetSubjectByIdAsync(int id)
        {
            var s = await _context.Subjects.Include(x => x.Teacher).FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return null;

            return new SubjectViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher != null ? s.Teacher.FullName : "Unassigned"
            };
        }

        public async Task AddSubjectAsync(SubjectViewModel model)
        {
            var subject = new Subject
            {
                Name = model.Name,
                Description = model.Description,
                TeacherId = model.TeacherId
            };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubjectAsync(SubjectViewModel model)
        {
            var subject = await _context.Subjects.FindAsync(model.Id);
            if (subject != null)
            {
                subject.Name = model.Name;
                subject.Description = model.Description;
                subject.TeacherId = model.TeacherId;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task DeleteSubjectAsync(int id)
        {
             var subject = await _context.Subjects.FindAsync(id);
             if (subject != null)
             {
                 _context.Subjects.Remove(subject);
                 await _context.SaveChangesAsync();
             }
        }

        // --- Grades ---
        public async Task AddGradeAsync(GradeViewModel model)
        {
            var grade = new Grade
            {
                StudentId = model.StudentId,
                SubjectId = model.SubjectId,
                SubjectName = model.SubjectName, // Keep name for display if needed, or fetch from DB
                Value = model.Value
            };
            
            // If SubjectId is provided but Name is empty, fetch name
            if (model.SubjectId.HasValue && string.IsNullOrEmpty(grade.SubjectName))
            {
                var subject = await _context.Subjects.FindAsync(model.SubjectId);
                if (subject != null) grade.SubjectName = subject.Name;
            }

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();
        }
        
        public async Task<List<GradeViewModel>> GetGradesForStudentAsync(int studentId)
        {
            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentId == studentId)
                .ToListAsync();
                
            return grades.Select(g => new GradeViewModel
            {
                Id = g.Id,
                StudentId = g.StudentId,
                SubjectId = g.SubjectId,
                SubjectName = g.Subject != null ? g.Subject.Name : g.SubjectName, // Prefer relation
                Value = g.Value
            }).ToList();
        }

        // --- Absences ---
        public async Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused)
        {
            var absence = new Absence
            {
                StudentId = studentId,
                SubjectId = subjectId,
                IsExcused = isExcused,
                Date = DateTime.Now
            };
            _context.Absences.Add(absence);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Absence>> GetAbsencesForStudentAsync(int studentId)
        {
            return await _context.Absences
                .Include(a => a.Subject)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        // --- RBAC Helpers ---
        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
        {
            return await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        // --- Messaging ---
        public async Task<List<Message>> GetMessagesForUserAsync(string userId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<List<Message>> GetSentMessagesAsync(string userId)
        {
             return await _context.Messages
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }

        public async Task SendMessageAsync(string senderId, string receiverEmail, string subject, string content)
        {
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Email == receiverEmail);
            if (receiver == null) throw new Exception("Receiver not found");

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiver.Id,
                Subject = subject,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
        
        // Overload using ID directly if needed
        public async Task SendMessageByIdAsync(string senderId, string receiverId, string subject, string content)
        {
             var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Subject = subject,
                Content = content,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            var msg = await _context.Messages.FindAsync(messageId);
            if (msg != null)
            {
                msg.IsRead = true;
                await _context.SaveChangesAsync();
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
