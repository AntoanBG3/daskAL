using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface IGradeService
    {
        Task AddGradeAsync(GradeViewModel model);
        Task UpdateGradeAsync(GradeViewModel model);
        Task DeleteGradeAsync(int id);
        Task<List<GradeViewModel>> GetGradesForStudentAsync(int studentId);
    }
}
