using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public class AbsenceService
    {
        private readonly SchoolDbContext _context;

        public AbsenceService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused)
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

        public async Task<List<Absence>> GetAbsencesForStudentAsync(int studentId)
        {
            return await _context.Absences
                .Include(a => a.Subject)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
    }
}
