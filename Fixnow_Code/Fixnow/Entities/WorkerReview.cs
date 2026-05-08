namespace Fixnow.Entities;

/// <summary>
/// Review given by a Customer to a Worker after a Booking is completed.
/// </summary>
public class WorkerReview
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public Guid WorkerId { get; set; }
  public int Rating { get; set; }
  public string? Comment { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Customer { get; set; } = null!;
  public User Worker { get; set; } = null!;
}
