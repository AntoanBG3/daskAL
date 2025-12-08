using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagementSystem.Web.Models.Auth;

[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)] 
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Revoked { get; set; }
    public string? ReplacedByToken { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked == null && !IsExpired;

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }
}
