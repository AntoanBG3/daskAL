using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface IGradeService
    {
        Task AddGradeAsync(GradeViewModel model);
        Task<List<GradeViewModel>> GetGradesForStudentAsync(int studentId);
    }
}
