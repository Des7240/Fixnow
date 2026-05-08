using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Auth;

/// <summary>
/// DTO for user login request.
/// </summary>
public class LoginRequestDto
{
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  [Required]
  public string Password { get; set; } = string.Empty;
}
