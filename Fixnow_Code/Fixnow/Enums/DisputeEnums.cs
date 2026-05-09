namespace Fixnow.Enums;

public enum DisputeStatus
{
  OPEN,
  INVESTIGATING,
  RESOLVED,
  REFUNDED,
  REJECTED,
  CLOSED
}

public enum RefundType
{
  FULL_REFUND,
  PARTIAL_REFUND
}

public enum RefundStatus
{
  PENDING,
  PROCESSING,
  SUCCESS,
  FAILED
}
