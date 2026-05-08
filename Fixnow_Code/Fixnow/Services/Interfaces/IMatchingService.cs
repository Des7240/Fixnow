namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service interface for worker matching logic.
/// </summary>
public interface IMatchingService
{
  /// <summary>
  /// Finds nearby available workers and notifies them about the booking.
  /// Updates booking status to MATCHING.
  /// </summary>
  Task TriggerMatchingAsync(Guid bookingId);
}
