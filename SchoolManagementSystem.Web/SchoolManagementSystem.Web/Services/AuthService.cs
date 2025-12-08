using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.DTOs;
using SchoolManagementSystem.Web.Models;
using SchoolManagementSystem.Web.Models.Auth;

namespace SchoolManagementSystem.Web.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthService> _logger;
    private readonly SchoolDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AuthService> logger,
        SchoolDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Generic error message to prevent enumeration
            return (false, "Invalid login attempt.");
        }

        if (!user.IsActive)
        {
            await LogAttempt(request.Email, false, "InactiveAccount");
            return (false, "Your account is not active. Please contact support or wait for approval.");
        }

        var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await LogAttempt(request.Email, true, request.GetHashCode().ToString()); // Simplify context usage
            return (true, "Login successful.");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", request.Email);
            await LogAttempt(request.Email, false, "LockedOut");
            return (false, "Account is locked due to multiple failed attempts. Try again later.");
        }

        await LogAttempt(request.Email, false, "InvalidCredentials");
        return (false, "Invalid login attempt.");
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = false // Default to false for new registrations
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Student"); 

            // Create pending Student entity
            var student = new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                UserId = user.Id
                // SchoolClassId left null, assigned later
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return (true, Enumerable.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded) return (true, "Password changed successfully");
        
        return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }
    
    private async Task LogAttempt(string email, bool success, string? reason = null)
    {
        // Simple logging
        var attempt = new LoginAttempt
        {
            Email = email,
            WasSuccess = success,
            AttemptTime = DateTime.UtcNow,
            FailureReason = reason
        };
        _context.LoginAttempts.Add(attempt);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> UploadProfilePictureAsync(string userId, byte[] imageData, string contentType)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        user.PendingProfilePicture = imageData;
        user.PendingProfilePictureContentType = contentType;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return (true, "Profile picture uploaded successfully and awaiting approval.");

        return (false, "Failed to upload profile picture.");
    }

    public async Task<(bool Success, string Message)> ApproveProfilePictureAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        if (user.PendingProfilePicture == null) return (false, "No pending picture to approve.");

        user.ProfilePicture = user.PendingProfilePicture;
        user.ProfilePictureContentType = user.PendingProfilePictureContentType;
        user.PendingProfilePicture = null;
        user.PendingProfilePictureContentType = null;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return (true, "Profile picture approved.");
        return (false, "Failed to approve picture.");
    }

    public async Task<(bool Success, string Message)> RejectProfilePictureAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        user.PendingProfilePicture = null;
        user.PendingProfilePictureContentType = null;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return (true, "Profile picture rejected.");
        return (false, "Failed to reject picture.");
    }

    public async Task<(bool Success, string Message)> ApproveUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        user.IsActive = true;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) return (true, "User approved.");
        return (false, "Failed to approve user.");
    }

    public async Task<(bool Success, string Message)> RejectUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        // Delete associated Student/Teacher if needed (Cascade might handle it but let's be safe)
        // With CascadeDelete configured in EF for User -> Student?
        // Student has UserId but it's not a FK enforced by Identity automatically in the same way.
        // Let's check Student definition. public string? UserId. No explicit navigation property back from Student to User in Student class except manual join usually.
        // But DbSeeder links them.

        // Let's try to find linked student
        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        if (student != null)
        {
            _context.Students.Remove(student);
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
             await _context.SaveChangesAsync(); // Commit student deletion if any
             return (true, "User rejected and deleted.");
        }
        return (false, "Failed to delete user.");
    }
}
