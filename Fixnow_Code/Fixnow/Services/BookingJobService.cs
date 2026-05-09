using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class BookingJobService : IBookingJobService
{
  private readonly IBookingRepository _bookingRepo;
  private readonly IBookingMatchingLogRepository _matchingLogRepo;
  private readonly INotificationService _notificationService;
  private readonly IAuditService _auditService;

  public BookingJobService(
    IBookingRepository bookingRepo,
    IBookingMatchingLogRepository matchingLogRepo,
    INotificationService notificationService,
    IAuditService auditService)
  {
    _bookingRepo = bookingRepo;
    _matchingLogRepo = matchingLogRepo;
    _notificationService = notificationService;
    _auditService = auditService;
  }

  public async Task CancelExpiredBookingAsync(Guid bookingId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId);
    if (booking == null) return;

    // Check if it's still waiting
    if (booking.Status == BookingStatus.PENDING || booking.Status == BookingStatus.MATCHING)
    {
      booking.Status = BookingStatus.CANCELLED;
      await _bookingRepo.UpdateAsync(booking);

      // Expire logs
      await _matchingLogRepo.ExpireAllNotifiedAsync(bookingId);

      // Notify customer
      await _notificationService.NotifyCustomerBookingStatusAsync(booking.CustomerId, bookingId, "CANCELLED");

      // Audit log
      await _auditService.LogActionAsync("JOB_BOOKING_TIMEOUT", "Booking", null, "SYSTEM", bookingId, null, "Auto-cancelled due to timeout");
    }
  }
}
