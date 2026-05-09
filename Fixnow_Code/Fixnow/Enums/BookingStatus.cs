namespace Fixnow.Enums;

/// <summary>
/// Represents the lifecycle states of a booking order.
/// State machine: PENDING → MATCHING → ASSIGNED → ON_THE_WAY → WORKING → COMPLETED
/// Cancel allowed from: PENDING, ASSIGNED
/// </summary>
public enum BookingStatus
{
  PENDING,
  MATCHING,
  ASSIGNED,
  ON_THE_WAY,
  INSPECTING,
  QUOTED,
  QUOTE_APPROVED,
  QUOTE_REJECTED,
  WORKING,
  COMPLETED,
  CANCELLED
}
