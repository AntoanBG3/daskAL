using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Web.Models.Auth;

[Table("LoginAttempts")]
public class LoginAttempt
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    
    public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
    
    public bool WasSuccess { get; set; }
    
    [MaxLength(50)]
    public string? IpAddress { get; set; }
    
    [MaxLength(200)]
    public string? UserAgent { get; set; }
    
    public string? FailureReason { get; set; }
}
