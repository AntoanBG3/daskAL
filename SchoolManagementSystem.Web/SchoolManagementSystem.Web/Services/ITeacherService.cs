using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.ViewModels;

namespace SchoolManagementSystem.Web.Services
{
    public interface ITeacherService
    {
        Task<List<TeacherViewModel>> GetAllTeachersAsync();
        Task<TeacherViewModel?> GetTeacherByIdAsync(int id);
        Task AddTeacherAsync(TeacherViewModel model);
        Task AddTeacherAsync(TeacherViewModel model, string? userId);
        Task UpdateTeacherAsync(TeacherViewModel model);
        Task DeleteTeacherAsync(int id);
        Task<Teacher?> GetTeacherByUserIdAsync(string userId);
    }
}
