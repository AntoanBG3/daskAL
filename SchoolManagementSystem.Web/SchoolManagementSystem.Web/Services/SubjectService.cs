using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class SubjectService : BaseService<SubjectService>
    {
        private readonly SchoolDbContext _context;

        public SubjectService(SchoolDbContext context, ILogger<SubjectService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task<List<SubjectViewModel>> GetAllSubjectsAsync()
        {
            return await ExecuteSafeAsync(async () =>
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
            }, "Error occurred while retrieving all subjects.", new List<SubjectViewModel>());
        }

        public async Task<SubjectViewModel?> GetSubjectByIdAsync(int id)
        {
            return await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while retrieving subject with ID {id}", null);
        }

        public async Task AddSubjectAsync(SubjectViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var subject = new Subject
                {
                    Name = model.Name,
                    Description = model.Description,
                    TeacherId = model.TeacherId
                };
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }, $"Error occurred while adding subject {model.Name}");
        }

        public async Task UpdateSubjectAsync(SubjectViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var subject = await _context.Subjects.FindAsync(model.Id);
                if (subject != null)
                {
                    subject.Name = model.Name;
                    subject.Description = model.Description;
                    subject.TeacherId = model.TeacherId;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating subject {model.Id}");
        }
        
        public async Task DeleteSubjectAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                 var subject = await _context.Subjects.FindAsync(id);
                 if (subject != null)
                 {
                     _context.Subjects.Remove(subject);
                     await _context.SaveChangesAsync();
                 }
            }, $"Error occurred while deleting subject {id}");
        }
        
        public async Task<List<SubjectViewModel>> GetAvailableSubjectsForStudentAsync(int studentId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null || student.SchoolClassId == null) return new List<SubjectViewModel>();
                
                var subjects = await _context.ClassSubjects
                    .Where(cs => cs.SchoolClassId == student.SchoolClassId && cs.Subject != null)
                    .Include(cs => cs.Subject)
                    .ThenInclude(s => s!.Teacher)
                    .Select(cs => cs.Subject!)
                    .ToListAsync();
                    
                return subjects.Select(s => new SubjectViewModel
                {
                    Id = s.Id,
                    Name = s.Name, 
                    TeacherName = s.Teacher != null ? s.Teacher.FullName : "Unassigned"
                }).ToList();
            }, $"Error occurred while retrieving available subjects for student {studentId}", new List<SubjectViewModel>());
        }
    }
}
