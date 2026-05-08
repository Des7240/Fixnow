using Fixnow.Enums;

namespace Fixnow.Entities;

/// <summary>
/// Worker profile details, 1-to-1 with User.
/// </summary>
public class WorkerProfile
{
  public Guid UserId { get; set; }
  public string? Bio { get; set; }
  public int ExperienceYears { get; set; }
  public double AverageRating { get; set; } = 0;
  public int TotalJobs { get; set; } = 0;
  public WorkerAvailability AvailabilityStatus { get; set; } = WorkerAvailability.OFFLINE;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User User { get; set; } = null!;
}
