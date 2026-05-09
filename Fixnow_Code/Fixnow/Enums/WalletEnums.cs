namespace Fixnow.Enums;

public enum TransactionType
{
  BOOKING_INCOME,
  COMMISSION_FEE,
  WITHDRAWAL,
  REFUND,
  DEPOSIT,
  ADJUSTMENT
}

public enum WithdrawalStatus
{
  PENDING,
  PROCESSING,
  SUCCESS,
  FAILED,
  CANCELLED
}
