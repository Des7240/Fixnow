using Fixnow.Enums;

namespace Fixnow.Entities;

/// <summary>
/// Logs each worker that was notified for a booking and their response.
/// </summary>
public class BookingMatchingLog
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid WorkerId { get; set; }
  public double DistanceMeters { get; set; }
  public MatchingLogStatus Status { get; set; } = MatchingLogStatus.NOTIFIED;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Worker { get; set; } = null!;
}
