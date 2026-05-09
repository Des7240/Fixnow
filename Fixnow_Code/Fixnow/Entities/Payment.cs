using Fixnow.Enums;

namespace Fixnow.Entities;

public enum PaymentType { BOOKING, WALLET_DEPOSIT }

public class Payment
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid? BookingId { get; set; }
  public Guid CustomerId { get; set; } // For wallet deposit, this is the User (Worker/Customer)
  public PaymentProvider Provider { get; set; }
  public decimal Amount { get; set; }
  public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
  public PaymentType Type { get; set; } = PaymentType.BOOKING;
  public string? TransactionCode { get; set; } // Provider's transaction code
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking? Booking { get; set; }
  public User Customer { get; set; } = null!;
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
