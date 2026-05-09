using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <summary>
/// Implements worker matching logic: finds nearby available workers and notifies them.
/// </summary>
public class MatchingService : IMatchingService
{
  private readonly IBookingRepository _bookingRepo;
  private readonly IWorkerLocationRepository _workerLocationRepo;
  private readonly IBookingMatchingLogRepository _matchingLogRepo;
  private readonly INotificationService _notificationService;
  private readonly IBookingStatusHistoryRepository _historyRepo;

  public MatchingService(
    IBookingRepository bookingRepo,
    IWorkerLocationRepository workerLocationRepo,
    IBookingMatchingLogRepository matchingLogRepo,
    INotificationService notificationService,
    IBookingStatusHistoryRepository historyRepo)
  {
    _bookingRepo = bookingRepo;
    _workerLocationRepo = workerLocationRepo;
    _matchingLogRepo = matchingLogRepo;
    _notificationService = notificationService;
    _historyRepo = historyRepo;
  }

  /// <inheritdoc/>
  public async Task TriggerMatchingAsync(Guid bookingId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

    var oldStatus = booking.Status;

    // Find nearby available workers within 10km, matching the requested ServiceId
    var nearbyWorkers = await _workerLocationRepo.FindNearbyAvailableWorkersAsync(
      booking.Lat, booking.Lng, booking.ServiceId, radiusMeters: 10000, limit: 20);

    // Update booking status to MATCHING (even if no workers found yet)
    booking.Status = BookingStatus.MATCHING;
    await _bookingRepo.UpdateAsync(booking);

    // Log status history (system updated, so we use Guid.Empty or a System user ID if available)
    await _historyRepo.AddAsync(new BookingStatusHistory
    {
      BookingId = bookingId,
      OldStatus = oldStatus,
      NewStatus = BookingStatus.MATCHING,
      UpdatedBy = Guid.Empty, // System
      CreatedAt = DateTime.UtcNow
    });

    if (nearbyWorkers.Count == 0)
      return;

    // Create matching log and notify each worker
    foreach (var worker in nearbyWorkers)
    {
      var log = new BookingMatchingLog
      {
        BookingId = bookingId,
        WorkerId = worker.WorkerId,
        DistanceMeters = worker.DistanceMeters,
        Status = MatchingLogStatus.NOTIFIED,
      };

      await _matchingLogRepo.CreateAsync(log);
      await _notificationService.NotifyWorkerNewBookingAsync(
        worker.WorkerId, bookingId, booking.Service.Name);
    }
  }
}
