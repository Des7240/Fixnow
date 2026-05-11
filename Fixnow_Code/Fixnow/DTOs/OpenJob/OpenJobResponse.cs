using Fixnow.Enums;

namespace Fixnow.DTOs.OpenJob;

public class OpenJobResponse
{
  public Guid Id { get; set; }
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string? CustomerAvatar { get; set; }
  public Guid ServiceId { get; set; }
  public string ServiceName { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }
  public int RadiusKm { get; set; }
  public decimal? MinBudget { get; set; }
  public decimal? MaxBudget { get; set; }
  public string? UrgencyLevel { get; set; }
  public DateTime? ExpiresAt { get; set; }
  public OpenJobStatus Status { get; set; }
  public ModerationStatus ModerationStatus { get; set; }
  public int ReportCount { get; set; }
  public DateTime CreatedAt { get; set; }
  public List<string> FileUrls { get; set; } = new();
  public int OfferCount { get; set; }
  public double? DistanceKm { get; set; }
}
