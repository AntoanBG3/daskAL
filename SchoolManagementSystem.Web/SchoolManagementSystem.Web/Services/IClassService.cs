using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface IClassService
    {
        Task<List<SchoolClassViewModel>> GetAllClassesAsync();
        Task<SchoolClassViewModel?> GetClassByIdAsync(int id);
        Task AddClassAsync(SchoolClassViewModel model);
        Task UpdateClassAsync(SchoolClassViewModel model);
        Task UpdateClassSubjectsAsync(int classId, List<int> subjectIds);
        Task DeleteClassAsync(int id);
    }
}
