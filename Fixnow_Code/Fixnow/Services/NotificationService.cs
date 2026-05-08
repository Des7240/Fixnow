using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <summary>
/// Console-based notification placeholder.
/// Replace with Firebase Admin SDK implementation for production.
/// </summary>
public class NotificationService : INotificationService
{
  private readonly ILogger<NotificationService> _logger;

  public NotificationService(ILogger<NotificationService> logger)
  {
    _logger = logger;
  }

  /// <inheritdoc/>
  public Task NotifyWorkerNewBookingAsync(Guid workerId, Guid bookingId, string serviceName)
  {
    _logger.LogInformation(
      "[NOTIFY] Worker {WorkerId} → New booking {BookingId} for service '{Service}'",
      workerId, bookingId, serviceName);
    return Task.CompletedTask;
  }

  /// <inheritdoc/>
  public Task NotifyCustomerBookingAssignedAsync(Guid customerId, Guid bookingId, string workerName)
  {
    _logger.LogInformation(
      "[NOTIFY] Customer {CustomerId} → Booking {BookingId} assigned to worker '{Worker}'",
      customerId, bookingId, workerName);
    return Task.CompletedTask;
  }

  /// <inheritdoc/>
  public Task NotifyCustomerBookingStatusAsync(Guid customerId, Guid bookingId, string status)
  {
    _logger.LogInformation(
      "[NOTIFY] Customer {CustomerId} → Booking {BookingId} status changed to '{Status}'",
      customerId, bookingId, status);
    return Task.CompletedTask;
  }
}
