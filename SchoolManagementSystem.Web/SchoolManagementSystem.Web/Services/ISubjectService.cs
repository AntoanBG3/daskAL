using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface ISubjectService
    {
        Task<List<SubjectViewModel>> GetAllSubjectsAsync();
        Task<SubjectViewModel?> GetSubjectByIdAsync(int id);
        Task AddSubjectAsync(SubjectViewModel model);
        Task UpdateSubjectAsync(SubjectViewModel model);
        Task DeleteSubjectAsync(int id);
        Task<List<SubjectViewModel>> GetAvailableSubjectsForStudentAsync(int studentId);
    }
}
