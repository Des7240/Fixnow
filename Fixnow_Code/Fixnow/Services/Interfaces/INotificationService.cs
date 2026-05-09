namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service interface for sending push notifications to workers or customers.
/// In MVP this is a placeholder; swap with Firebase FCM implementation later.
/// </summary>
public interface INotificationService
{
  /// <summary>Notifies a worker about a new booking opportunity.</summary>
  Task NotifyWorkerNewBookingAsync(Guid workerId, Guid bookingId, string serviceName);

  /// <summary>Notifies a customer that a worker has accepted their booking.</summary>
  Task NotifyCustomerBookingAssignedAsync(Guid customerId, Guid bookingId, string workerName);

  /// <summary>Notifies a customer that their booking was cancelled or no worker found.</summary>
  Task NotifyCustomerBookingStatusAsync(Guid customerId, Guid bookingId, string status);

  /// <summary>Notifies a user about a new chat message.</summary>
  Task NotifyNewChatMessageAsync(Guid userId, Guid bookingId, string senderName, string messagePreview);
}
