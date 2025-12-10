using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class GradeService : BaseService<GradeService>
    {
        private readonly SchoolDbContext _context;

        public GradeService(SchoolDbContext context, ILogger<GradeService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task AddGradeAsync(GradeViewModel model)
        {
            await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while adding grade for student {model.StudentId}");
        }
        
        public async Task<List<GradeViewModel>> GetGradesForStudentAsync(int studentId)
        {
            return await ExecuteSafeAsync(async () =>
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
            }, $"Error occurred while retrieving grades for student {studentId}", new List<GradeViewModel>());
        }
    }
}
