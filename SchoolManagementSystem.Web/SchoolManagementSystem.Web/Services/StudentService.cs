using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.Auth;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class StudentService : BaseService<StudentService>, IStudentService
    {
        private readonly SchoolDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudentService(SchoolDbContext context, ILogger<StudentService> logger, UserManager<User> userManager) : base(logger)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<StudentViewModel>> GetAllStudentsAsync()
        {
            return await ExecuteSafeAsync(async () =>
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
                    ClassId = s.SchoolClassId,
                    DateOfBirth = s.DateOfBirth,
                    AverageGrade = s.Grades.Any() ? s.Grades.Average(g => g.Value) : 0
                }).ToList();
            }, "Error occurred while retrieving all students.", new List<StudentViewModel>());
        }

        public async Task AddStudentAsync(StudentViewModel model, int classId)
        {
            await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while adding student {model.FirstName} {model.LastName}");
        }
        
        public async Task AddStudentAsync(StudentViewModel model)
        {
            if (model.ClassId.HasValue)
            {
                await AddStudentAsync(model, model.ClassId.Value);
            }
            else if (int.TryParse(model.Class, out int classId))
            {
                await AddStudentAsync(model, classId);
            }
            else
            {
                throw new ArgumentException("Invalid Class ID provided.");
            }
        }

        public async Task<StudentViewModel?> GetStudentByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async () =>
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
                    ClassId = s.SchoolClassId,
                    DateOfBirth = s.DateOfBirth,
                    AverageGrade = s.Grades.Any() ? s.Grades.Average(g => g.Value) : 0
                };
            }, $"Error occurred while retrieving student with ID {id}", null);
        }

        public async Task UpdateStudentAsync(StudentViewModel model, int classId)
        {
            await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while updating student {model.Id}");
        }

        public async Task DeleteStudentAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                var student = await _context.Students.FindAsync(id);
                if (student != null)
                {
                    // Deactivate the linked User account if it exists
                    if (!string.IsNullOrEmpty(student.UserId))
                    {
                        var user = await _userManager.FindByIdAsync(student.UserId);
                        if (user != null)
                        {
                            user.IsActive = false;
                            await _userManager.UpdateAsync(user);
                        }
                    }

                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while deleting student {id}");
        }
        
        public async Task<Student?> GetStudentByUserIdAsync(string userId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.Students
                    .Include(s => s.SchoolClass)
                    .FirstOrDefaultAsync(s => s.UserId == userId);
            }, $"Error occurred while retrieving student by UserID {userId}", null);
        }
    }
}
