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

  /// <summary>Notifies a worker that an assigned booking was cancelled.</summary>
  Task NotifyWorkerBookingStatusAsync(Guid workerId, Guid bookingId, string status);

  /// <summary>Notifies a user about a new chat message.</summary>
  Task NotifyNewChatMessageAsync(Guid userId, Guid bookingId, string senderName, string messagePreview);

  /// <summary>Notifies a worker about a new open job opportunity nearby.</summary>
  Task NotifyWorkerNewOpenJobAsync(Guid workerId, Guid jobId, string title);

  /// <summary>Notifies a customer that a worker has submitted an offer for their open job.</summary>
  Task NotifyCustomerNewOfferAsync(Guid customerId, Guid jobId, string workerName);

  /// <summary>Notifies a worker that their offer has been accepted.</summary>
  Task NotifyWorkerOfferAcceptedAsync(Guid workerId, Guid bookingId);
}
