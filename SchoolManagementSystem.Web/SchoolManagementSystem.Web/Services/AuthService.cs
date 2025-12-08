using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Web.Data;
using SchoolManagementSystem.Web.DTOs;
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
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            // Assign default role - for now 'Student' or 'Staff' could be logic based.
            // Requirement says RBAC: Admin, Teacher, Staff, Student.
            // We'll Default to Student for public registration, Admin must assign others.
            // Or if first user? logic can be added.
            
            // await _userManager.AddToRoleAsync(user, "Student"); 
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
}
