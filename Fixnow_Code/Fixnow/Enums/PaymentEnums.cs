namespace Fixnow.Enums;

public enum PaymentStatus
{
  PENDING,
  PROCESSING,
  SUCCESS,
  FAILED,
  CANCELLED,
  REFUNDED
}

public enum BookingPaymentStatus
{
  UNPAID,
  PARTIALLY_PAID,
  PAID,
  REFUNDED
}

public enum PaymentProvider
{
  VNPAY,
  MOMO
}
