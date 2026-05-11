using Fixnow.Enums;

namespace Fixnow.Entities;

public class WorkerOffer
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid OpenJobId { get; set; }
  public Guid WorkerId { get; set; }
  public decimal EstimatedPrice { get; set; }
  public string Analysis { get; set; } = string.Empty;
  public int EstimatedArrivalMinutes { get; set; }
  public int EstimatedRepairTimeMinutes { get; set; }
  public int? WarrantyDays { get; set; }
  public OfferStatus Status { get; set; } = OfferStatus.SUBMITTED;
  public ModerationStatus ModerationStatus { get; set; } = ModerationStatus.PENDING;
  public decimal SpamScore { get; set; } = 0;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public OpenJob OpenJob { get; set; } = null!;
  public User Worker { get; set; } = null!;
  public ICollection<OfferAttachment> Attachments { get; set; } = new List<OfferAttachment>();
}
