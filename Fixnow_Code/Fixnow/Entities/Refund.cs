using Fixnow.Enums;

namespace Fixnow.Entities;

public class Refund
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid DisputeId { get; set; }
  
  public decimal Amount { get; set; }
  public RefundType RefundType { get; set; } = RefundType.FULL_REFUND;
  public RefundStatus Status { get; set; } = RefundStatus.PENDING;
  
  public Guid? ProcessedBy { get; set; }
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Dispute Dispute { get; set; } = null!;
  public User? AdminProcessor { get; set; }
}
