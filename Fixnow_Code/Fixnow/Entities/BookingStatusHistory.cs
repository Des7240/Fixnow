using Fixnow.Enums;

namespace Fixnow.Entities;

/// <summary>
/// Audit trail for every booking status transition.
/// </summary>
public class BookingStatusHistory
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public BookingStatus? OldStatus { get; set; }
  public BookingStatus NewStatus { get; set; }
  public Guid UpdatedBy { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
}
