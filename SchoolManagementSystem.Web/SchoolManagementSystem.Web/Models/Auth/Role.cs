using Microsoft.AspNetCore.Identity;

namespace SchoolManagementSystem.Web.Models.Auth;

public class Role : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    
    public Role() : base() { }
    public Role(string roleName) : base(roleName) { }
}
