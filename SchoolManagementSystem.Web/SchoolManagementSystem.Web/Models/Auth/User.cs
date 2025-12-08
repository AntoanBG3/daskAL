using Microsoft.AspNetCore.Identity;

namespace SchoolManagementSystem.Web.Models.Auth;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties for related data if needed
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
