using Fixnow.Enums;

namespace Fixnow.DTOs.OpenJob;

public class OfferResponse
{
  public Guid Id { get; set; }
  public Guid OpenJobId { get; set; }
  public Guid WorkerId { get; set; }
  public string WorkerName { get; set; } = string.Empty;
  public string? WorkerAvatar { get; set; }
  public decimal EstimatedPrice { get; set; }
  public string Analysis { get; set; } = string.Empty;
  public int EstimatedArrivalMinutes { get; set; }
  public int EstimatedRepairTimeMinutes { get; set; }
  public int? WarrantyDays { get; set; }
  public OfferStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public List<string> FileUrls { get; set; } = new();
  
  // Additional worker info for comparison
  public double WorkerRating { get; set; }
  public int WorkerCompletedJobs { get; set; }
  public double WorkerScore { get; set; }
}
