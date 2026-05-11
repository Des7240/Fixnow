namespace Fixnow.DTOs.Admin;

public class UserDto
{
  public Guid Id { get; set; }
  public string Email { get; set; } = string.Empty;
  public string FullName { get; set; } = string.Empty;
  public string? AvatarUrl { get; set; }
  public string Role { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
