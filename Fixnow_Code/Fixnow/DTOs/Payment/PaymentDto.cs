using Fixnow.Enums;

namespace Fixnow.DTOs.Payment;

public class CreatePaymentRequestDto
{
  public Guid BookingId { get; set; }
  public PaymentProvider Provider { get; set; }
}

public class CreatePaymentResponseDto
{
  public Guid PaymentId { get; set; }
  public string PaymentUrl { get; set; } = string.Empty;
}

public class PaymentResultDto
{
  public bool IsSuccess { get; set; }
  public string? TransactionId { get; set; }
  public string? ErrorMessage { get; set; }
  public string RawResponse { get; set; } = string.Empty;
  public decimal Amount { get; set; }
}

public class PaymentRequestDto
{
  public Guid PaymentId { get; set; }
  public Guid BookingId { get; set; }
  public decimal Amount { get; set; }
  public string Description { get; set; } = string.Empty;
  public string ReturnUrl { get; set; } = string.Empty;
  public string IpAddress { get; set; } = string.Empty;
}
