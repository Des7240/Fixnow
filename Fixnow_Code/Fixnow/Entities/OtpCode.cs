using System.ComponentModel.DataAnnotations;

namespace Fixnow.Entities;

public enum OtpType
{
    PASSWORD_RESET,
    WITHDRAWAL_VERIFICATION,
    LOGIN_VERIFICATION
}

public class OtpCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Code { get; set; } = string.Empty;
    
    public OtpType Type { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public bool IsUsed { get; set; } = false;
}
