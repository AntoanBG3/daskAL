using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SchoolManagementSystem
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Class { get; set; }
        public DateTime BirthDate { get; set; }
        public Dictionary<string, List<int>> Grades { get; set; } = new Dictionary<string, List<int>>();
    }

    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public List<string> Subjects { get; set; } = new List<string>();
    }

    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TeacherId { get; set; }
    }

    public class SchoolData
    {
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        public List<Subject> Subjects { get; set; } = new List<Subject>();
    }

    public class DataManager
    {
        private const string DataFile = "school_data.json";
        private SchoolData schoolData;

        public DataManager()
        {
            LoadData();
        }

        public void LoadData()
        {
            if (File.Exists(DataFile))
            {
                try
                {
                    string json = File.ReadAllText(DataFile);
                    schoolData = JsonSerializer.Deserialize<SchoolData>(json) ?? new SchoolData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Грешка при зареждане на данните: {ex.Message}");
                    schoolData = new SchoolData();
                }
            }
            else
            {
                schoolData = new SchoolData();
            }
        }

        public void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(schoolData, options);
                File.WriteAllText(DataFile, json);
                Console.WriteLine("Данните са запазени успешно.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при запазване на данните: {ex.Message}");
            }
        }

        public void AddStudent(Student student)
        {
            student.Id = schoolData.Students.Count > 0 ? schoolData.Students.Max(s => s.Id) + 1 : 1;
            schoolData.Students.Add(student);
        }

        public void AddTeacher(Teacher teacher)
        {
            teacher.Id = schoolData.Teachers.Count > 0 ? schoolData.Teachers.Max(t => t.Id) + 1 : 1;
            schoolData.Teachers.Add(teacher);
        }

        public void AddSubject(Subject subject)
        {
            subject.Id = schoolData.Subjects.Count > 0 ? schoolData.Subjects.Max(s => s.Id) + 1 : 1;
            schoolData.Subjects.Add(subject);
        }

        public void AddGrade(int studentId, string subject, int grade)
        {
            var student = schoolData.Students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                if (!student.Grades.ContainsKey(subject))
                {
                    student.Grades[subject] = new List<int>();
                }
                student.Grades[subject].Add(grade);
            }
        }

        public List<Student> GetStudents() => schoolData.Students;
        public List<Teacher> GetTeachers() => schoolData.Teachers;
        public List<Subject> GetSubjects() => schoolData.Subjects;
    }

    class Program
    {
        private static DataManager dataManager;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            dataManager = new DataManager();
            
            while (true)
            {
                ShowMenu();
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        AddStudent();
                        break;
                    case "2":
                        AddTeacher();
                        break;
                    case "3":
                        AddSubject();
                        break;
                    case "4":
                        AddGrade();
                        break;
                    case "5":
                        ShowStudents();
                        break;
                    case "6":
                        ShowTeachers();
                        break;
                    case "7":
                        ShowSubjects();
                        break;
                    case "8":
                        ShowStudentGrades();
                        break;
                    case "9":
                        dataManager.SaveData();
                        break;
                    case "0":
                        dataManager.SaveData();
                        return;
                    default:
                        Console.WriteLine("Невалиден избор!");
                        break;
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("\n=== СИСТЕМА ЗА УПРАВЛЕНИЕ НА УЧИЛИЩЕ ===");
            Console.WriteLine("1. Добавяне на ученик");
            Console.WriteLine("2. Добавяне на учител");
            Console.WriteLine("3. Добавяне на предмет");
            Console.WriteLine("4. Добавяне на оценка");
            Console.WriteLine("5. Преглед на ученици");
            Console.WriteLine("6. Преглед на учители");
            Console.WriteLine("7. Преглед на предмети");
            Console.WriteLine("8. Преглед на оценки по ученик");
            Console.WriteLine("9. Запазване на данните");
            Console.WriteLine("0. Изход");
            Console.Write("Изберете опция: ");
        }

        static void AddStudent()
        {
            Console.WriteLine("\n=== ДОБАВЯНЕ НА УЧЕНИК ===");
            
            Console.Write("Име: ");
            string name = Console.ReadLine();
            
            Console.Write("Фамилия: ");
            string lastName = Console.ReadLine();
            
            Console.Write("Клас: ");
            string className = Console.ReadLine();
            
            Console.Write("Дата на раждане (дд.мм.гггг): ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime birthDate))
            {
                var student = new Student
                {
                    Name = name,
                    LastName = lastName,
                    Class = className,
                    BirthDate = birthDate
                };
                
                dataManager.AddStudent(student);
                Console.WriteLine("Ученикът е добавен успешно!");
            }
            else
            {
                Console.WriteLine("Невалиден формат на дата!");
            }
        }

        static void AddTeacher()
        {
            Console.WriteLine("\n=== ДОБАВЯНЕ НА УЧИТЕЛ ===");
            
            Console.Write("Име: ");
            string name = Console.ReadLine();
            
            Console.Write("Фамилия: ");
            string lastName = Console.ReadLine();
            
            Console.Write("Предмети (разделени със запетая): ");
            string subjectsInput = Console.ReadLine();
            var subjects = subjectsInput.Split(',').Select(s => s.Trim()).ToList();
            
            var teacher = new Teacher
            {
                Name = name,
                LastName = lastName,
                Subjects = subjects
            };
            
            dataManager.AddTeacher(teacher);
            Console.WriteLine("Учителят е добавен успешно!");
        }

        static void AddSubject()
        {
            Console.WriteLine("\n=== ДОБАВЯНЕ НА ПРЕДМЕТ ===");
            
            Console.Write("Име на предмет: ");
            string name = Console.ReadLine();
            
            Console.Write("Описание: ");
            string description = Console.ReadLine();
            
            Console.WriteLine("Налични учители:");
            var teachers = dataManager.GetTeachers();
            foreach (var teacher in teachers)
            {
                Console.WriteLine($"{teacher.Id}. {teacher.Name} {teacher.LastName}");
            }
            
            Console.Write("ID на учител: ");
            if (int.TryParse(Console.ReadLine(), out int teacherId))
            {
                var subject = new Subject
                {
                    Name = name,
                    Description = description,
                    TeacherId = teacherId
                };
                
                dataManager.AddSubject(subject);
                Console.WriteLine("Предметът е добавен успешно!");
            }
            else
            {
                Console.WriteLine("Невалиден ID на учител!");
            }
        }

        static void AddGrade()
        {
            Console.WriteLine("\n=== ДОБАВЯНЕ НА ОЦЕНКА ===");
            
            Console.WriteLine("Налични ученици:");
            var students = dataManager.GetStudents();
            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}. {student.Name} {student.LastName} - {student.Class}");
            }
            
            Console.Write("ID на ученик: ");
            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                Console.Write("Предмет: ");
                string subject = Console.ReadLine();
                
                Console.Write("Оценка (2-6): ");
                if (int.TryParse(Console.ReadLine(), out int grade) && grade >= 2 && grade <= 6)
                {
                    dataManager.AddGrade(studentId, subject, grade);
                    Console.WriteLine("Оценката е добавена успешно!");
                }
                else
                {
                    Console.WriteLine("Невалидна оценка! Трябва да бъде между 2 и 6.");
                }
            }
            else
            {
                Console.WriteLine("Невалиден ID на ученик!");
            }
        }

        static void ShowStudents()
        {
            Console.WriteLine("\n=== СПИСЪК НА УЧЕНИЦИ ===");
            var students = dataManager.GetStudents();
            
            if (students.Count == 0)
            {
                Console.WriteLine("Няма въведени ученици.");
                return;
            }
            
            foreach (var student in students)
            {
                Console.WriteLine($"ID: {student.Id}");
                Console.WriteLine($"Име: {student.Name} {student.LastName}");
                Console.WriteLine($"Клас: {student.Class}");
                Console.WriteLine($"Дата на раждане: {student.BirthDate:dd.MM.yyyy}");
                Console.WriteLine("---");
            }
        }

        static void ShowTeachers()
        {
            Console.WriteLine("\n=== СПИСЪК НА УЧИТЕЛИ ===");
            var teachers = dataManager.GetTeachers();
            
            if (teachers.Count == 0)
            {
                Console.WriteLine("Няма въведени учители.");
                return;
            }
            
            foreach (var teacher in teachers)
            {
                Console.WriteLine($"ID: {teacher.Id}");
                Console.WriteLine($"Име: {teacher.Name} {teacher.LastName}");
                Console.WriteLine($"Предмети: {string.Join(", ", teacher.Subjects)}");
                Console.WriteLine("---");
            }
        }

        static void ShowSubjects()
        {
            Console.WriteLine("\n=== СПИСЪК НА ПРЕДМЕТИ ===");
            var subjects = dataManager.GetSubjects();
            var teachers = dataManager.GetTeachers();
            
            if (subjects.Count == 0)
            {
                Console.WriteLine("Няма въведени предмети.");
                return;
            }
            
            foreach (var subject in subjects)
            {
                var teacher = teachers.FirstOrDefault(t => t.Id == subject.TeacherId);
                Console.WriteLine($"ID: {subject.Id}");
                Console.WriteLine($"Предмет: {subject.Name}");
                Console.WriteLine($"Описание: {subject.Description}");
                Console.WriteLine($"Учител: {teacher?.Name} {teacher?.LastName}");
                Console.WriteLine("---");
            }
        }

        static void ShowStudentGrades()
        {
            Console.WriteLine("\n=== ОЦЕНКИ НА УЧЕНИК ===");
            var students = dataManager.GetStudents();
            
            if (students.Count == 0)
            {
                Console.WriteLine("Няма въведени ученици.");
                return;
            }
            
            Console.WriteLine("Налични ученици:");
            foreach (var student in students)
            {
                Console.WriteLine($"{student.Id}. {student.Name} {student.LastName} - {student.Class}");
            }
            
            Console.Write("ID на ученик: ");
            if (int.TryParse(Console.ReadLine(), out int studentId))
            {
                var student = students.FirstOrDefault(s => s.Id == studentId);
                if (student != null)
                {
                    Console.WriteLine($"\nОценки на {student.Name} {student.LastName}:");
                    if (student.Grades.Count == 0)
                    {
                        Console.WriteLine("Няма въведени оценки.");
                    }
                    else
                    {
                        foreach (var subject in student.Grades)
                        {
                            double average = subject.Value.Average();
                            Console.WriteLine($"{subject.Key}: {string.Join(", ", subject.Value)} (Средна: {average:F2})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Ученик с този ID не е намерен!");
                }
            }
        }
    }
}