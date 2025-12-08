using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models.Auth;

namespace SchoolManagementSystem.Web.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> LoginAsync(LoginRequest request);
    Task LogoutAsync();
    Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    // Profile Picture Methods
    Task<(bool Success, string Message)> UploadProfilePictureAsync(string userId, byte[] imageData, string contentType);
    Task<(bool Success, string Message)> ApproveProfilePictureAsync(string userId);
    Task<(bool Success, string Message)> RejectProfilePictureAsync(string userId);

    // User Approval Methods
    Task<(bool Success, string Message)> ApproveUserAsync(string userId);
    Task<(bool Success, string Message)> RejectUserAsync(string userId);
}
