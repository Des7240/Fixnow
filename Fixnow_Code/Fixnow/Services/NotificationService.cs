using Fixnow.Entities;
using Fixnow.Hubs;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.SignalR;

namespace Fixnow.Services;

/// <summary>
/// Enhanced notification service using SignalR for real-time and Hangfire for reliability.
/// </summary>
public class NotificationService : INotificationService
{
  private readonly INotificationRepository _notificationRepo;
  private readonly IHubContext<NotificationHub> _hubContext;
  private readonly IBackgroundJobClient _backgroundJobClient;
  private readonly ILogger<NotificationService> _logger;

  public NotificationService(
    INotificationRepository notificationRepo,
    ILogger<NotificationService> logger,
    IBackgroundJobClient backgroundJobClient,
    IHubContext<NotificationHub> hubContext)
  {
    _notificationRepo = notificationRepo;
    _logger = logger;
    _backgroundJobClient = backgroundJobClient;
    _hubContext = hubContext;
  }

  /// <inheritdoc/>
  public Task NotifyWorkerNewBookingAsync(Guid workerId, Guid bookingId, string serviceName)
  {
    _backgroundJobClient.Enqueue(() => DoNotifyWorkerNewBookingAsync(workerId, bookingId, serviceName));
    return Task.CompletedTask;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task DoNotifyWorkerNewBookingAsync(Guid workerId, Guid bookingId, string serviceName)
  {
    _logger.LogInformation(
      "[NOTIFY] Worker {WorkerId} → New booking {BookingId} for service '{Service}'",
      workerId, bookingId, serviceName);

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = workerId,
      Title = "Đơn mới đang chờ bạn!",
      Content = $"Có đơn dịch vụ '{serviceName}' mới cần thợ. Nhấn để xem chi tiết.",
      Type = "NEW_BOOKING",
      ReferenceId = bookingId
    });

    // Push real-time notification via SignalR
    await _hubContext.Clients.User(workerId.ToString()).SendAsync("ReceiveBookingMatch", new
    {
      BookingId = bookingId,
      ServiceName = serviceName,
      Message = "Có đơn hàng mới phù hợp với bạn!"
    });
  }

  /// <inheritdoc/>
  public Task NotifyCustomerBookingAssignedAsync(Guid customerId, Guid bookingId, string workerName)
  {
    _backgroundJobClient.Enqueue(() => DoNotifyCustomerBookingAssignedAsync(customerId, bookingId, workerName));
    return Task.CompletedTask;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task DoNotifyCustomerBookingAssignedAsync(Guid customerId, Guid bookingId, string workerName)
  {
    _logger.LogInformation(
      "[NOTIFY] Customer {CustomerId} → Booking {BookingId} assigned to worker '{Worker}'",
      customerId, bookingId, workerName);

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = customerId,
      Title = "Đã tìm được thợ cho bạn!",
      Content = $"Thợ {workerName} đã nhận đơn của bạn và sẽ sớm liên hệ.",
      Type = "BOOKING_ACCEPTED",
      ReferenceId = bookingId
    });

    await _hubContext.Clients.User(customerId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = "BOOKING_ACCEPTED",
      ReferenceId = bookingId,
      Message = $"Thợ {workerName} đã nhận đơn của bạn."
    });
  }

  /// <inheritdoc/>
  public Task NotifyCustomerBookingStatusAsync(Guid customerId, Guid bookingId, string status)
  {
    _backgroundJobClient.Enqueue(() => DoNotifyCustomerBookingStatusAsync(customerId, bookingId, status));
    return Task.CompletedTask;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task DoNotifyCustomerBookingStatusAsync(Guid customerId, Guid bookingId, string status)
  {
    _logger.LogInformation(
      "[NOTIFY] Customer {CustomerId} → Booking {BookingId} status changed to '{Status}'",
      customerId, bookingId, status);

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = customerId,
      Title = "Cập nhật đơn hàng",
      Content = $"Đơn hàng của bạn đã chuyển sang trạng thái: {status}",
      Type = "BOOKING_STATUS_UPDATE",
      ReferenceId = bookingId
    });

    await _hubContext.Clients.User(customerId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = "BOOKING_STATUS_UPDATE",
      ReferenceId = bookingId,
      Message = $"Đơn hàng của bạn hiện là: {status}"
    });
  }

  /// <inheritdoc/>
  public Task NotifyWorkerBookingStatusAsync(Guid workerId, Guid bookingId, string status)
  {
    _backgroundJobClient.Enqueue(() => DoNotifyWorkerBookingStatusAsync(workerId, bookingId, status));
    return Task.CompletedTask;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task DoNotifyWorkerBookingStatusAsync(Guid workerId, Guid bookingId, string status)
  {
    _logger.LogInformation(
      "[NOTIFY] Worker {WorkerId} → Booking {BookingId} status changed to '{Status}'",
      workerId, bookingId, status);

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = workerId,
      Title = "Cập nhật đơn hàng",
      Content = $"Đơn hàng bạn đang nhận đã chuyển sang trạng thái: {status}",
      Type = "BOOKING_STATUS_UPDATE",
      ReferenceId = bookingId
    });

    await _hubContext.Clients.User(workerId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = "BOOKING_STATUS_UPDATE",
      ReferenceId = bookingId,
      Message = $"Đơn hàng đang nhận hiện là: {status}"
    });
  }

  /// <inheritdoc/>
  public async Task NotifyNewChatMessageAsync(Guid userId, Guid bookingId, string senderName, string messagePreview)
  {
    // Skip persistent notification storage for chat to avoid clutter
    await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = "CHAT_MESSAGE",
      ReferenceId = bookingId,
      SenderName = senderName,
      Message = messagePreview
    });
  }
}
