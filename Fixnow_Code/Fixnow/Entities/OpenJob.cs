using Fixnow.Enums;
using NetTopologySuite.Geometries;

namespace Fixnow.Entities;

public class OpenJob
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid CustomerId { get; set; }
  public Guid ServiceId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }
  
  /// <summary>PostGIS geography(Point, 4326) for geo queries.</summary>
  public Point Location { get; set; } = null!;
  
  public int RadiusKm { get; set; }
  public decimal? MinBudget { get; set; }
  public decimal? MaxBudget { get; set; }
  public string? UrgencyLevel { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public OpenJobStatus Status { get; set; } = OpenJobStatus.OPEN;
  public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.PENDING;
  public string? ModerationReason { get; set; }
  public DateTime? ModeratedAt { get; set; }
  public Guid? ModeratedBy { get; set; }
  public int ReportCount { get; set; } = 0;
  public string? ClosedReason { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  // Navigation
  public User Customer { get; set; } = null!;
  public ServiceCategory Service { get; set; } = null!;
  public ICollection<OpenJobAttachment> Attachments { get; set; } = new List<OpenJobAttachment>();
  public ICollection<WorkerOffer> Offers { get; set; } = new List<WorkerOffer>();
}
