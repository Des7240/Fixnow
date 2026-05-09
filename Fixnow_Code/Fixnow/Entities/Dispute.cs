using Fixnow.Enums;

namespace Fixnow.Entities;

public class Dispute
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public Guid WorkerId { get; set; }
  
  public string Reason { get; set; } = string.Empty;
  public DisputeStatus Status { get; set; } = DisputeStatus.OPEN;
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Customer { get; set; } = null!;
  public User Worker { get; set; } = null!;
  public ICollection<DisputeEvidence> Evidences { get; set; } = new List<DisputeEvidence>();
  public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
