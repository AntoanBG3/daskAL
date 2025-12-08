using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class TeacherService
    {
        private readonly SchoolDbContext _context;

        public TeacherService(SchoolDbContext context)
        {
            _context = context;
        }

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

        public async Task<Teacher?> GetTeacherByUserIdAsync(string userId)
        {
            return await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }
    }
}
