using Fixnow.Enums;

namespace Fixnow.Entities;

public class Quotation
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid WorkerId { get; set; }
  public Guid CustomerId { get; set; }
  
  public decimal Subtotal { get; set; }
  public decimal TotalAmount { get; set; }
  public string? Note { get; set; }
  public QuotationStatus Status { get; set; } = QuotationStatus.PENDING;
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime ExpiresAt { get; set; }

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Worker { get; set; } = null!;
  public User Customer { get; set; } = null!;
  public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
