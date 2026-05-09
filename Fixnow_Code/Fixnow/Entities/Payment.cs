using Fixnow.Enums;

namespace Fixnow.Entities;

public class Payment
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public PaymentProvider Provider { get; set; }
  public decimal Amount { get; set; }
  public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
  public string? TransactionCode { get; set; } // Provider's transaction code
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Customer { get; set; } = null!;
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
