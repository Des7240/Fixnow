namespace Fixnow.Entities;

/// <summary>
/// Represents a refresh token issued to a user.
/// </summary>
public class RefreshToken
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  public string Token { get; set; } = string.Empty;
  public DateTime ExpiresAt { get; set; }
  public bool Revoked { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User User { get; set; } = null!;
}
