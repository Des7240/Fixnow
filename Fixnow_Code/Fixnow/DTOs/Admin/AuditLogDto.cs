namespace Fixnow.DTOs.Admin;

public class AuditLogDto
{
  public Guid Id { get; set; }
  public Guid? ActorId { get; set; }
  public string? ActorRole { get; set; }
  public string Action { get; set; } = string.Empty;
  public string EntityType { get; set; } = string.Empty;
  public Guid? EntityId { get; set; }
  public string? OldData { get; set; }
  public string? NewData { get; set; }
  public string? IpAddress { get; set; }
  public DateTime CreatedAt { get; set; }
}
