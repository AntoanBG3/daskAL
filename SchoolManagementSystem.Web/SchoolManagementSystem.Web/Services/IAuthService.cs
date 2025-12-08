using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models.Auth;

namespace SchoolManagementSystem.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}
