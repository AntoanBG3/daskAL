using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class StudentService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<StudentService> _logger;

        public StudentService(SchoolDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<StudentViewModel>> GetAllStudentsAsync()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all students.");
                return new List<StudentViewModel>();
            }
        }

        public async Task AddStudentAsync(StudentViewModel model, int classId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding student {StudentName}", model.FirstName + " " + model.LastName);
                throw;
            }
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student with ID {StudentId}", id);
                return null;
            }
        }

        public async Task UpdateStudentAsync(StudentViewModel model, int classId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating student {StudentId}", model.Id);
                throw;
            }
        }

        public async Task DeleteStudentAsync(int id)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting student {StudentId}", id);
                throw;
            }
        }
        
        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Students
                    .Include(s => s.SchoolClass)
                    .FirstOrDefaultAsync(s => s.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving student by UserID {UserId}", userId);
                return null;
            }
        }
    }
}
