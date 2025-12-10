using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class TeacherService : BaseService<TeacherService>
    {
        private readonly SchoolDbContext _context;

        public TeacherService(SchoolDbContext context, ILogger<TeacherService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task<List<TeacherViewModel>> GetAllTeachersAsync()
        {
            return await ExecuteSafeAsync(async () =>
            {
                var teachers = await _context.Teachers.Include(t => t.TeachingSubjects).ToListAsync();
                return teachers.Select(t => new TeacherViewModel
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    TeachingSubjects = t.TeachingSubjects.Select(s => s.Name).ToList()
                }).ToList();
            }, "Error occurred while retrieving all teachers.", new List<TeacherViewModel>());
        }

        public async Task<TeacherViewModel?> GetTeacherByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while retrieving teacher with ID {id}", null);
        }

        public async Task AddTeacherAsync(TeacherViewModel model)
        {
            await AddTeacherAsync(model, null);
        }

        public async Task AddTeacherAsync(TeacherViewModel model, string? userId)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserId = userId
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
            }, $"Error occurred while adding teacher {model.FirstName} {model.LastName}");
        }

        public async Task UpdateTeacherAsync(TeacherViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = await _context.Teachers.FindAsync(model.Id);
                if (teacher != null)
                {
                    teacher.FirstName = model.FirstName;
                    teacher.LastName = model.LastName;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating teacher {model.Id}");
        }
        
        public async Task DeleteTeacherAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher != null)
                {
                    _context.Teachers.Remove(teacher);
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while deleting teacher {id}");
        }

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == userId);
            }, $"Error occurred while retrieving teacher by UserID {userId}", null);
        }
    }
}
