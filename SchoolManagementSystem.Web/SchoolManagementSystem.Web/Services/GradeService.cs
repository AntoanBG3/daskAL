using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class GradeService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<GradeService> _logger;

        public GradeService(SchoolDbContext context, ILogger<GradeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddGradeAsync(GradeViewModel model)
        {
            try
            {
                var grade = new Grade
                {
                    StudentId = model.StudentId,
                    SubjectId = model.SubjectId,
                    SubjectName = model.SubjectName, // Keep name for display if needed, or fetch from DB
                    Value = model.Value
                };
                
                // If SubjectId is provided but Name is empty, fetch name
                if (model.SubjectId.HasValue && string.IsNullOrEmpty(grade.SubjectName))
                {
                    var subject = await _context.Subjects.FindAsync(model.SubjectId);
                    if (subject != null) grade.SubjectName = subject.Name;
                }

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding grade for student {StudentId}", model.StudentId);
                throw;
            }
        }
        
        public async Task<List<GradeViewModel>> GetGradesForStudentAsync(int studentId)
        {
            try
            {
                var grades = await _context.Grades
                    .Include(g => g.Subject)
                    .Where(g => g.StudentId == studentId)
                    .ToListAsync();
                    
                return grades.Select(g => new GradeViewModel
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    SubjectId = g.SubjectId,
                    SubjectName = g.Subject != null ? g.Subject.Name : g.SubjectName, // Prefer relation
                    Value = g.Value
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving grades for student {StudentId}", studentId);
                return new List<GradeViewModel>();
            }
        }
    }
}
