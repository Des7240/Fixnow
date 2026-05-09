namespace Fixnow.DTOs.Notification;

public class NotificationDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public bool IsRead { get; set; }
  public Guid? ReferenceId { get; set; }
  public DateTime CreatedAt { get; set; }
}
