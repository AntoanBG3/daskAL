using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class TeacherService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(SchoolDbContext context, ILogger<TeacherService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TeacherViewModel>> GetAllTeachersAsync()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all teachers.");
                return new List<TeacherViewModel>();
            }
        }

        public async Task<TeacherViewModel?> GetTeacherByIdAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving teacher with ID {TeacherId}", id);
                return null;
            }
        }

        public async Task AddTeacherAsync(TeacherViewModel model)
        {
            try
            {
                var teacher = new Teacher
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding teacher {TeacherName}", model.FirstName + " " + model.LastName);
                throw;
            }
        }

        public async Task UpdateTeacherAsync(TeacherViewModel model)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(model.Id);
                if (teacher != null)
                {
                    teacher.FirstName = model.FirstName;
                    teacher.LastName = model.LastName;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating teacher {TeacherId}", model.Id);
                throw;
            }
        }
        
        public async Task DeleteTeacherAsync(int id)
        {
            try
            {
                var teacher = await _context.Teachers.FindAsync(id);
                if (teacher != null)
                {
                    _context.Teachers.Remove(teacher);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting teacher {TeacherId}", id);
                throw;
            }
        }

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
        {
            try
            {
                return await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving teacher by UserID {UserId}", userId);
                return null;
            }
        }
    }
}
