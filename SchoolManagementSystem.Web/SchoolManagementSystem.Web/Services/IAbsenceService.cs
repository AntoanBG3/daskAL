using SchoolManagementSystem.Web.Models;

namespace SchoolManagementSystem.Web.Services
{
    public interface IAbsenceService
    {
        Task AddAbsenceAsync(int studentId, int? subjectId, bool isExcused, DateTime? date = null);
        Task UpdateAbsenceAsync(int id, bool isExcused);
        Task DeleteAbsenceAsync(int id);
        Task<List<Absence>> GetAbsencesForStudentAsync(int studentId);
    }
}
