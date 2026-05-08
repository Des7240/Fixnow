namespace Fixnow.DTOs.Auth;

/// <summary>
/// DTO returned after successful authentication.
/// </summary>
public class AuthResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public int ExpiresIn { get; set; }
  public UserInfoDto User { get; set; } = null!;
}

/// <summary>
/// Basic user info included in auth response.
/// </summary>
public class UserInfoDto
{
  public Guid Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public string Role { get; set; } = string.Empty;
}
