using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface IStudentService
    {
        Task<List<StudentViewModel>> GetAllStudentsAsync();
        Task<StudentViewModel?> GetStudentByIdAsync(int id);
        Task AddStudentAsync(StudentViewModel model);
        Task AddStudentAsync(StudentViewModel model, int classId);
        Task UpdateStudentAsync(StudentViewModel model, int classId);
        Task DeleteStudentAsync(int id);
        Task<Student?> GetStudentByUserIdAsync(string userId);
    }
}
