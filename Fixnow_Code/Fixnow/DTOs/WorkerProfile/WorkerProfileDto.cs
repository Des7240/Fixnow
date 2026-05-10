using Fixnow.Enums;

namespace Fixnow.DTOs.WorkerProfile;

/// <summary>
/// Response DTO for Worker Profile.
/// </summary>
public class WorkerProfileDto
{
  public Guid UserId { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Bio { get; set; }
  public int ExperienceYears { get; set; }
  public double AverageRating { get; set; }
  public int TotalJobs { get; set; }
  public WorkerAvailability AvailabilityStatus { get; set; }
  public List<WorkerServiceDto> Skills { get; set; } = new();
}

public class WorkerServiceDto
{
  public Guid ServiceId { get; set; }
  public string ServiceName { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
}
