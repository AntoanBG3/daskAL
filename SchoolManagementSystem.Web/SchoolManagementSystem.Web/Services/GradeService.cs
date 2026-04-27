using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public class GradeService : BaseService<GradeService>, IGradeService
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
                    Value = model.Value
                };

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
                    SubjectName = g.Subject?.Name ?? "Unknown",
                    Value = g.Value
                }).ToList();
            }, $"Error occurred while retrieving grades for student {studentId}", new List<GradeViewModel>());
        }
        public async Task UpdateGradeAsync(GradeViewModel model)
        {
            await ExecuteSafeAsync(async () =>
            {
                var grade = await _context.Grades.FindAsync(model.Id);
                if (grade != null)
                {
                    grade.Value = model.Value;
                    grade.SubjectId = model.SubjectId;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating grade {model.Id}");
        }

        public async Task DeleteGradeAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                var grade = await _context.Grades.FindAsync(id);
                if (grade != null)
                {
                    _context.Grades.Remove(grade);
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while deleting grade {id}");
        }
    }
}
