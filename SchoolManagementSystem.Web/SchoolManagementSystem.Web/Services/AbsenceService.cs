using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class AbsenceService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<AbsenceService> _logger;

        public AbsenceService(SchoolDbContext context, ILogger<AbsenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused)
        {
            try
            {
                var absence = new Absence
                {
                    StudentId = studentId,
                    SubjectId = subjectId,
                    IsExcused = isExcused,
                    Date = DateTime.Now
                };
                _context.Absences.Add(absence);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding absence for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<List<Absence>> GetAbsencesForStudentAsync(int studentId)
        {
            try
            {
                return await _context.Absences
                    .Include(a => a.Subject)
                    .Where(a => a.StudentId == studentId)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving absences for student {StudentId}", studentId);
                return new List<Absence>();
            }
        }
    }
}
