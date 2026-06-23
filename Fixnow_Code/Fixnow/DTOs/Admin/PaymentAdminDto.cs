namespace Fixnow.DTOs.Admin;

public class PaymentAdminDto
{
  public Guid Id { get; set; }
  public decimal Amount { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Provider { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string? TransactionCode { get; set; }
  public DateTime CreatedAt { get; set; }

  // Customer info
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public string CustomerEmail { get; set; } = string.Empty;
}
