using System.ComponentModel.DataAnnotations.Schema;

namespace Fixnow.Entities;

/// <summary>
/// Audit trail for business actions.
/// </summary>
public class AuditLog
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid? ActorId { get; set; }
  public string? ActorRole { get; set; }
  public string Action { get; set; } = string.Empty; // e.g. LOGIN_SUCCESS, BOOKING_CANCELLED
  public string EntityType { get; set; } = string.Empty; // e.g. User, Booking
  public Guid? EntityId { get; set; }
  
  [Column(TypeName = "jsonb")]
  public string? OldData { get; set; }
  
  [Column(TypeName = "jsonb")]
  public string? NewData { get; set; }
  
  public string? IpAddress { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
