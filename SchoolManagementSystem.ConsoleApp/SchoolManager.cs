using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SchoolManagementSystem
{
    public class SchoolManager
    {
        private const string DATA_FILE = "school_data.json";
        private SchoolDatabase _database = new();

        public SchoolManager()
        {
            LoadDatabase();
        }

        private void LoadDatabase()
        {
            try
            {
                if (File.Exists(DATA_FILE))
                {
                    string jsonContent = File.ReadAllText(DATA_FILE);
                    _database = JsonSerializer.Deserialize<SchoolDatabase>(jsonContent) ?? new SchoolDatabase();
                    Console.WriteLine("📚 School data loaded successfully!");
                }
                else
                {
                    _database = new SchoolDatabase();
                    Console.WriteLine("🆕 Starting with a new school database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading data: {ex.Message}");
                Console.WriteLine("Starting with an empty database.");
                _database = new SchoolDatabase();
            }
        }

        public void SaveDatabase()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string jsonContent = JsonSerializer.Serialize(_database, jsonOptions);
                File.WriteAllText(DATA_FILE, jsonContent);
                Console.WriteLine("✅ Data saved successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving data: {ex.Message}");
            }
        }

        public void RegisterStudent(Student student)
        {
            student.Id = GetNextStudentId();
            _database.Students.Add(student);
            Console.WriteLine($"✅ Student {student.FullName} registered successfully!");
        }

        public void RegisterTeacher(Teacher teacher)
        {
            teacher.Id = GetNextTeacherId();
            _database.Teachers.Add(teacher);
            Console.WriteLine($"✅ Teacher {teacher.FullName} registered successfully!");
        }

        public void AddSubject(Subject subject)
        {
            subject.Id = GetNextSubjectId();
            _database.Subjects.Add(subject);
            Console.WriteLine($"✅ Subject '{subject.Name}' added successfully!");
        }

        public bool RecordGrade(int studentId, string subjectName, int grade)
        {
            var student = _database.Students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                Console.WriteLine("❌ Student not found!");
                return false;
            }

            if (!_database.Subjects.Any(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Subject '{subjectName}' not found! Please add the subject first.");
                return false;
            }

            if (!student.SubjectGrades.ContainsKey(subjectName))
            {
                student.SubjectGrades[subjectName] = new List<int>();
            }

            student.SubjectGrades[subjectName].Add(grade);
            Console.WriteLine($"✅ Grade {grade} recorded for {student.FullName} in {subjectName}");
            return true;
        }

        public bool AssignSubjectToTeacher(int teacherId, string subjectName)
        {
            // Check if subject exists in database
            var subject = _database.Subjects.FirstOrDefault(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
            if (subject == null)
            {
                Console.WriteLine($"❌ Subject '{subjectName}' not found! Please add it from the main menu first.");
                return false;
            }

            var teacher = _database.Teachers.FirstOrDefault(t => t.Id == teacherId);
            if (teacher == null)
            {
                Console.WriteLine("❌ Teacher not found!");
                return false;
            }

            if (teacher.TeachingSubjects.Any(s => s.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Teacher {teacher.FullName} already teaches {subjectName}!");
                return false;
            }
            
            // Assign subject to teacher and teacher to subject
            subject.TeacherId = teacherId;
            teacher.TeachingSubjects.Add(subjectName);
            Console.WriteLine($"✅ Subject '{subjectName}' assigned to teacher {teacher.FullName}");
            return true;
        }

        public bool RemoveSubjectFromTeacher(int teacherId, string subjectName)
        {
            var teacher = _database.Teachers.FirstOrDefault(t => t.Id == teacherId);
            if (teacher == null)
            {
                Console.WriteLine("❌ Teacher not found!");
                return false;
            }

            var subjectToRemove = teacher.TeachingSubjects.FirstOrDefault(s => s.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
            if (subjectToRemove == null)
            {
                Console.WriteLine($"❌ Teacher {teacher.FullName} does not teach {subjectName}!");
                return false;
            }

            teacher.TeachingSubjects.Remove(subjectToRemove);
            
            // Also unassign teacher from subject
            var subject = _database.Subjects.FirstOrDefault(s => s.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase));
            if (subject != null && subject.TeacherId == teacherId)
            {
                subject.TeacherId = null;
            }

            Console.WriteLine($"✅ Subject '{subjectName}' removed from teacher {teacher.FullName}");
            return true;
        }

        private int GetNextStudentId() => _database.Students.Any() ? _database.Students.Max(s => s.Id) + 1 : 1;
        private int GetNextTeacherId() => _database.Teachers.Any() ? _database.Teachers.Max(t => t.Id) + 1 : 1;
        private int GetNextSubjectId() => _database.Subjects.Any() ? _database.Subjects.Max(s => s.Id) + 1 : 1;

        public List<Student> GetAllStudents() => _database.Students.ToList();
        public List<Teacher> GetAllTeachers() => _database.Teachers.ToList();
        public List<Subject> GetAllSubjects() => _database.Subjects.ToList();

        public Student? FindStudent(int id) => _database.Students.FirstOrDefault(s => s.Id == id);
        public Teacher? FindTeacher(int id) => _database.Teachers.FirstOrDefault(t => t.Id == id);
    }
}
