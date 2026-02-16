using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public interface IAbsenceService
    {
        Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused);
        Task<List<Absence>> GetAbsencesForStudentAsync(int studentId);
    }
}
