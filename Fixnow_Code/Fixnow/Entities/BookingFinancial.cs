namespace Fixnow.Entities;

public class BookingFinancial
{
  public Guid BookingId { get; set; }
  public decimal TotalAmount { get; set; }
  public decimal PlatformFee { get; set; }
  public decimal WorkerIncome { get; set; }

  // Navigation
  public Booking Booking { get; set; } = null!;
}
