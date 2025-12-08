using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class StudentService
    {
        private readonly SchoolDbContext _context;

        public StudentService(SchoolDbContext context)
        {
            _context = context;
        }

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
                Class = s.SchoolClass?.Name ?? "Unassigned",
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
        
        public async Task AddStudentAsync(StudentViewModel model)
        {
            if (int.TryParse(model.Class, out int classId))
            {
                await AddStudentAsync(model, classId);
            }
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
        
        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.SchoolClass)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }
}
