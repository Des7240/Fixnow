namespace Fixnow.Entities;

/// <summary>
/// Aggregated rating summary for a worker. Updated on each new review.
/// </summary>
public class WorkerRatingSummary
{
  public Guid WorkerId { get; set; }
  public double AverageRating { get; set; } = 0;
  public int TotalReviews { get; set; } = 0;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User Worker { get; set; } = null!;
}
