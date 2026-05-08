using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.Auth;

/// <summary>
/// DTO for user registration request.
/// </summary>
public class RegisterRequestDto
{
  [Required]
  [EmailAddress]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MinLength(6)]
  public string Password { get; set; } = string.Empty;

  [Required]
  public string FullName { get; set; } = string.Empty;

  [Required]
  public UserRole Role { get; set; } = UserRole.CUSTOMER;
}
