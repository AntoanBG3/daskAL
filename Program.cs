using System;
using System.Linq;

namespace SchoolManagementSystem
{
    class Program
    {
        private static readonly SchoolManager _schoolManager = new();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            ShowWelcomeMessage();
            
            while (true)
            {
                ShowMainMenu();
                
                string? choice = Console.ReadLine()?.Trim();
                
                if (string.IsNullOrEmpty(choice))
                {
                    Console.WriteLine("Please enter a valid option.");
                    continue;
                }

                bool shouldContinue = HandleUserChoice(choice);
                if (!shouldContinue) break;
            }
        }

        private static void ShowWelcomeMessage()
        {
            Console.WriteLine("🏫 Welcome to daskAL!");
            Console.WriteLine("This system helps you manage students, teachers, and grades.");
            Console.WriteLine("Let's get started!\n");
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("\n📋 What would you like to do?");
            Console.WriteLine("1️⃣  Add new student");
            Console.WriteLine("2️⃣  Add new teacher");
            Console.WriteLine("3️⃣  Add new subject");
            Console.WriteLine("4️⃣  Record grade");
            Console.WriteLine("5️⃣  View all students");
            Console.WriteLine("6️⃣  View all teachers");
            Console.WriteLine("7️⃣  View all subjects");
            Console.WriteLine("8️⃣  View student grades");
            Console.WriteLine("9️⃣  Assign subject to teacher");
            Console.WriteLine("🔟 Remove subject from teacher");
            Console.WriteLine("1️⃣1️⃣ Save data");
            Console.WriteLine("0️⃣  Exit");
            Console.Write("\nYour choice: ");
        }

        private static bool HandleUserChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    AddNewStudent();
                    break;
                case "2":
                    AddNewTeacher();
                    break;
                case "3":
                    AddNewSubject();
                    break;
                case "4":
                    RecordStudentGrade();
                    break;
                case "5":
                    ShowAllStudents();
                    break;
                case "6":
                    ShowAllTeachers();
                    break;
                case "7":
                    ShowAllSubjects();
                    break;
                case "8":
                    ShowStudentGrades();
                    break;
                case "9":
                    AssignSubjectToTeacher();
                    break;
                case "10":
                    RemoveSubjectFromTeacher();
                    break;
                case "11":
                    _schoolManager.SaveDatabase();
                    break;
                case "0":
                    HandleExit();
                    return false;
                default:
                    Console.WriteLine("❌ Invalid choice. Please try again.");
                    break;
            }
            return true;
        }

        private static void AddNewStudent()
        {
            Console.WriteLine("\n👤 Adding new student...");
            
            string firstName = GetUserInput("First Name: ");
            string lastName = GetUserInput("Last Name: ");
            string grade = GetUserInput("Grade/Class: ");
            
            Console.Write("Date of Birth (dd.MM.yyyy): ");
            string? dateInput = Console.ReadLine();
            
            if (DateTime.TryParseExact(dateInput, "dd.MM.yyyy", null, 
                System.Globalization.DateTimeStyles.None, out DateTime dateOfBirth))
            {
                var newStudent = new Student
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Class = grade,
                    DateOfBirth = dateOfBirth
                };
                
                _schoolManager.RegisterStudent(newStudent);
            }
            else
            {
                Console.WriteLine("❌ Invalid date format. Please use dd.MM.yyyy format.");
            }
        }

        private static void AddNewTeacher()
        {
            Console.WriteLine("\n👨‍🏫 Adding new teacher...");
            
            string firstName = GetUserInput("First Name: ");
            string lastName = GetUserInput("Last Name: ");
            
            var newTeacher = new Teacher
            {
                FirstName = firstName,
                LastName = lastName
            };
            
            _schoolManager.RegisterTeacher(newTeacher);
        }

        private static void AddNewSubject()
        {
            Console.WriteLine("\n📚 Adding new subject...");
            
            string subjectName = GetUserInput("Subject Name: ");
            string description = GetUserInput("Description: ");

            if (_schoolManager.GetAllSubjects().Any(p => p.Name.Equals(subjectName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"❌ Subject with name '{subjectName}' already exists.");
                return;
            }
            
            var newSubject = new Subject
            {
                Name = subjectName,
                Description = description
            };
            
            _schoolManager.AddSubject(newSubject);
            Console.WriteLine("💡 You can assign a teacher to this subject later from the menu (option 9).");
        }

        private static void RecordStudentGrade()
        {
            Console.WriteLine("\n📝 Recording grade...");
            
            var students = _schoolManager.GetAllStudents();
            if (!students.Any())
            {
                Console.WriteLine("❌ No students available. Please add students first.");
                return;
            }
            
            Console.WriteLine("\nStudents:");
            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}. {student.FullName} (Class: {student.Class})");
            }
            
            Console.Write("Student ID: ");
            if (!int.TryParse(Console.ReadLine(), out int studentId))
            {
                Console.WriteLine("❌ Invalid Student ID!");
                return;
            }
            
            string subjectName = GetUserInput("Subject: ");
            
            Console.Write("Grade (2-6): ");
            if (int.TryParse(Console.ReadLine(), out int grade) && grade >= 2 && grade <= 6)
            {
                _schoolManager.RecordGrade(studentId, subjectName, grade);
            }
            else
            {
                Console.WriteLine("❌ Invalid grade! Must be between 2 and 6.");
            }
        }

        private static void AssignSubjectToTeacher()
        {
            Console.WriteLine("\n👨‍🏫 Assigning subject to teacher...");

            var teachers = _schoolManager.GetAllTeachers();
            if (!teachers.Any())
            {
                Console.WriteLine("❌ No teachers available. Please add teachers first.");
                return;
            }

            var subjects = _schoolManager.GetAllSubjects();
            if (!subjects.Any())
            {
                Console.WriteLine("❌ No subjects available. Please add subjects first.");
                return;
            }

            Console.WriteLine("\nTeachers:");
            foreach (var teacher in teachers)
            {
                string teachingSubjects = teacher.TeachingSubjects.Any()
                    ? string.Join(", ", teacher.TeachingSubjects)
                    : "No subjects assigned";
                Console.WriteLine($"{teacher.Id}. {teacher.FullName} ({teachingSubjects})");
            }

            Console.Write("Teacher ID: ");
            if (!int.TryParse(Console.ReadLine(), out int teacherId))
            {
                Console.WriteLine("❌ Invalid Teacher ID!");
                return;
            }
            
            Console.WriteLine("\nAvailable Subjects:");
            foreach (var subject in subjects)
            {
                Console.WriteLine($"- {subject.Name}");
            }

            string subjectName = GetUserInput("Subject name to assign: ");

            _schoolManager.AssignSubjectToTeacher(teacherId, subjectName);
        }

        private static void RemoveSubjectFromTeacher()
        {
            Console.WriteLine("\n👨‍🏫 Removing subject from teacher...");
            
            var teachers = _schoolManager.GetAllTeachers();
            if (!teachers.Any())
            {
                Console.WriteLine("❌ No teachers available.");
                return;
            }
            
            var teachersWithSubjects = teachers.Where(u => u.TeachingSubjects.Any()).ToList();
            if (!teachersWithSubjects.Any())
            {
                Console.WriteLine("❌ No teachers with assigned subjects.");
                return;
            }
            
            Console.WriteLine("\nTeachers with subjects:");
            foreach (var teacher in teachersWithSubjects)
            {
                Console.WriteLine($"{teacher.Id}. {teacher.FullName} ({string.Join(", ", teacher.TeachingSubjects)})");
            }
            
            Console.Write("Teacher ID: ");
            if (!int.TryParse(Console.ReadLine(), out int teacherId))
            {
                Console.WriteLine("❌ Invalid Teacher ID!");
                return;
            }
            
            var selectedTeacher = teachers.FirstOrDefault(u => u.Id == teacherId);
            if (selectedTeacher == null || !selectedTeacher.TeachingSubjects.Any())
            {
                Console.WriteLine("❌ Teacher not found or has no assigned subjects!");
                return;
            }
            
            Console.WriteLine($"\nSubjects for {selectedTeacher.FullName}:");
            for (int i = 0; i < selectedTeacher.TeachingSubjects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {selectedTeacher.TeachingSubjects[i]}");
            }
            
            Console.Write("Subject number to remove: ");
            if (int.TryParse(Console.ReadLine(), out int subjectIndex) && 
                subjectIndex >= 1 && subjectIndex <= selectedTeacher.TeachingSubjects.Count)
            {
                string subjectName = selectedTeacher.TeachingSubjects[subjectIndex - 1];
                _schoolManager.RemoveSubjectFromTeacher(teacherId, subjectName);
            }
            else
            {
                Console.WriteLine("❌ Invalid subject number!");
            }
        }

        private static void ShowAllStudents()
        {
            Console.WriteLine("\n👥 All Students:");
            var students = _schoolManager.GetAllStudents();
            
            if (!students.Any())
            {
                Console.WriteLine("No registered students yet.");
                return;
            }
            
            foreach (var student in students)
            {
                Console.WriteLine($"\n📋 ID: {student.Id}");
                Console.WriteLine($"👤 Name: {student.FullName}");
                Console.WriteLine($"🎓 Class: {student.Class}");
                Console.WriteLine($"🎂 Age: {student.Age} years");
                Console.WriteLine($"📈 Overall Average: {student.GetOverallAverageGrade():F2}");
                Console.WriteLine("─────────────────────");
            }
        }

        private static void ShowAllTeachers()
        {
            Console.WriteLine("\n👨‍🏫 All Teachers:");
            var teachers = _schoolManager.GetAllTeachers();
            
            if (!teachers.Any())
            {
                Console.WriteLine("No registered teachers yet.");
                return;
            }
            
            foreach (var teacher in teachers)
            {
                Console.WriteLine($"\n📋 ID: {teacher.Id}");
                Console.WriteLine($"👤 Name: {teacher.FullName}");
                
                if (teacher.TeachingSubjects.Any())
                {
                    Console.WriteLine($"📚 Subjects: {string.Join(", ", teacher.TeachingSubjects)}");
                }
                else
                {
                    Console.WriteLine("📚 Subjects: No subjects assigned");
                }
                
                Console.WriteLine("─────────────────────");
            }
        }

        private static void ShowAllSubjects()
        {
            Console.WriteLine("\n📚 All Subjects:");
            var subjects = _schoolManager.GetAllSubjects();
            var teachers = _schoolManager.GetAllTeachers();
            
            if (!subjects.Any())
            {
                Console.WriteLine("No subjects added yet.");
                return;
            }
            
            foreach (var subject in subjects)
            {
                var teacher = subject.TeacherId.HasValue ? teachers.FirstOrDefault(u => u.Id == subject.TeacherId.Value) : null;
                Console.WriteLine($"\n📋 ID: {subject.Id}");
                Console.WriteLine($"📖 Subject: {subject.Name}");
                Console.WriteLine($"📝 Description: {subject.Description}");
                Console.WriteLine($"👨‍🏫 Teacher: {teacher?.FullName ?? "Unassigned"}");
                Console.WriteLine("─────────────────────");
            }
        }

        private static void ShowStudentGrades()
        {
            Console.WriteLine("\n📊 Student Grades:");
            var students = _schoolManager.GetAllStudents();
            
            if (!students.Any())
            {
                Console.WriteLine("No registered students yet.");
                return;
            }
            
            Console.WriteLine("\nStudents:");
            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}. {student.FullName} (Class: {student.Class})");
            }
            
            Console.Write("Student ID: ");
            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                var student = _schoolManager.FindStudent(studentId);
                if (student != null)
                {
                    Console.WriteLine($"\n📊 Grades for {student.FullName}:");
                    
                    if (!student.SubjectGrades.Any())
                    {
                        Console.WriteLine("No grades recorded yet.");
                        return;
                    }
                    
                    foreach (var subjectGrades in student.SubjectGrades)
                    {
                        var average = subjectGrades.Value.Average();
                        Console.WriteLine($"📚 {subjectGrades.Key}: {string.Join(", ", subjectGrades.Value)} (Average: {average:F2})");
                    }
                    
                    Console.WriteLine($"\n🎯 Overall Average: {student.GetOverallAverageGrade():F2}");
                }
                else
                {
                    Console.WriteLine("❌ Student not found!");
                }
            }
            else
            {
                Console.WriteLine("❌ Invalid Student ID!");
            }
        }

        private static void HandleExit()
        {
            Console.Write("💾 Save data before exit? (y/n): ");
            string? saveChoice = Console.ReadLine()?.ToLower();
            
            if (saveChoice == "y" || saveChoice == "yes")
            {
                _schoolManager.SaveDatabase();
            }
            
            Console.WriteLine("👋 Thank you for using daskAL");
            Console.WriteLine("Have a nice day!");
        }

        private static string GetUserInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }
    }
}
