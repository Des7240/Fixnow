using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Auth;

/// <summary>
/// DTO for refresh token request.
/// </summary>
public class RefreshTokenRequestDto
{
  [Required]
  public string RefreshToken { get; set; } = string.Empty;
}
