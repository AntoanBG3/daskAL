using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class SubjectService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<SubjectService> _logger;

        public SubjectService(SchoolDbContext context, ILogger<SubjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SubjectViewModel>> GetAllSubjectsAsync()
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all subjects.");
                return new List<SubjectViewModel>();
            }
        }

        public async Task<SubjectViewModel?> GetSubjectByIdAsync(int id)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving subject with ID {SubjectId}", id);
                return null;
            }
        }

        public async Task AddSubjectAsync(SubjectViewModel model)
        {
            try
            {
                var subject = new Subject
                {
                    Name = model.Name,
                    Description = model.Description,
                    TeacherId = model.TeacherId
                };
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding subject {SubjectName}", model.Name);
                throw;
            }
        }

        public async Task UpdateSubjectAsync(SubjectViewModel model)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(model.Id);
                if (subject != null)
                {
                    subject.Name = model.Name;
                    subject.Description = model.Description;
                    subject.TeacherId = model.TeacherId;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating subject {SubjectId}", model.Id);
                throw;
            }
        }
        
        public async Task DeleteSubjectAsync(int id)
        {
            try
            {
                 var subject = await _context.Subjects.FindAsync(id);
                 if (subject != null)
                 {
                     _context.Subjects.Remove(subject);
                     await _context.SaveChangesAsync();
                 }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting subject {SubjectId}", id);
                throw;
            }
        }
        
        public async Task<List<SubjectViewModel>> GetAvailableSubjectsForStudentAsync(int studentId)
        {
            try
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving available subjects for student {StudentId}", studentId);
                return new List<SubjectViewModel>();
            }
        }
    }
}
