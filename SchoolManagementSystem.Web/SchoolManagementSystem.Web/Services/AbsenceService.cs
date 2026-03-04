using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class AbsenceService : BaseService<AbsenceService>, IAbsenceService
    {
        private readonly SchoolDbContext _context;

        public AbsenceService(SchoolDbContext context, ILogger<AbsenceService> logger) : base(logger)
        {
            _context = context;
        }

        public async Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused, DateTime? date = null)
        {
            await ExecuteSafeAsync(async () =>
            {
                var absence = new Absence
                {
                    StudentId = studentId,
                    SubjectId = subjectId,
                    IsExcused = isExcused,
                    Date = date ?? DateTime.UtcNow
                };
                _context.Absences.Add(absence);
                await _context.SaveChangesAsync();
            }, $"Error occurred while adding absence for student {studentId}");
        }

        public async Task UpdateAbsenceAsync(int id, bool isExcused)
        {
            await ExecuteSafeAsync(async () =>
            {
                var absence = await _context.Absences.FindAsync(id);
                if (absence != null)
                {
                    absence.IsExcused = isExcused;
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while updating absence {id}");
        }

        public async Task DeleteAbsenceAsync(int id)
        {
            await ExecuteSafeAsync(async () =>
            {
                var absence = await _context.Absences.FindAsync(id);
                if (absence != null)
                {
                    _context.Absences.Remove(absence);
                    await _context.SaveChangesAsync();
                }
            }, $"Error occurred while deleting absence {id}");
        }

        public async Task<List<Absence>> GetAbsencesForStudentAsync(int studentId)
        {
            return await ExecuteSafeAsync(async () =>
            {
                return await _context.Absences
                    .Include(a => a.Subject)
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();
            }, $"Error occurred while retrieving absences for student {studentId}", new List<Absence>());
        }
    }
}
