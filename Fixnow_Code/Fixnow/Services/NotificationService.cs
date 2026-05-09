using Fixnow.Entities;
using Fixnow.Hubs;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.SignalR;

namespace Fixnow.Services;

/// <summary>
/// Enhanced notification service that writes in-app notifications to DB
/// in addition to console logging and real-time SignalR push.
/// Replace logger calls with Firebase FCM in production.
/// </summary>
public class NotificationService : INotificationService
{
  private readonly ILogger<NotificationService> _logger;
  private readonly INotificationRepository _notificationRepo;
  private readonly IBackgroundJobClient _backgroundJobClient;
  private readonly IHubContext<NotificationHub> _hubContext;

  public NotificationService(
    ILogger<NotificationService> logger,
    INotificationRepository notificationRepo,
    IBackgroundJobClient backgroundJobClient,
    IHubContext<NotificationHub> hubContext)
  {
    _logger = logger;
    _notificationRepo = notificationRepo;
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

    var (title, content) = status switch
    {
      "ON_THE_WAY" => ("Thợ đang trên đường đến!", "Thợ sẽ có mặt tại địa chỉ của bạn trong thời gian ngắn."),
      "WORKING"    => ("Thợ đang làm việc", "Công việc sửa chữa đang được thực hiện."),
      "COMPLETED"  => ("Hoàn thành! Hãy đánh giá dịch vụ", "Công việc đã hoàn thành. Hãy để lại đánh giá nhé!"),
      "CANCELLED"  => ("Đơn hàng đã bị huỷ", "Đơn hàng của bạn đã bị huỷ."),
      _ => ($"Trạng thái đơn: {status}", $"Đơn hàng của bạn hiện là: {status}")
    };

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = customerId,
      Title = title,
      Content = content,
      Type = $"BOOKING_{status}",
      ReferenceId = bookingId
    });

    await _hubContext.Clients.User(customerId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = $"BOOKING_{status}",
      ReferenceId = bookingId,
      Title = title,
      Message = content
    });
  }

  /// <inheritdoc/>
  public Task NotifyNewChatMessageAsync(Guid userId, Guid bookingId, string senderName, string messagePreview)
  {
    _backgroundJobClient.Enqueue(() => DoNotifyNewChatMessageAsync(userId, bookingId, senderName, messagePreview));
    return Task.CompletedTask;
  }

  [AutomaticRetry(Attempts = 3)]
  public async Task DoNotifyNewChatMessageAsync(Guid userId, Guid bookingId, string senderName, string messagePreview)
  {
    _logger.LogInformation(
      "[NOTIFY] User {UserId} → New chat message from '{Sender}'",
      userId, senderName);

    await _notificationRepo.AddAsync(new Notification
    {
      UserId = userId,
      Title = $"Tin nhắn mới từ {senderName}",
      Content = messagePreview,
      Type = "CHAT_MESSAGE",
      ReferenceId = bookingId
    });

    await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", new
    {
      Type = "CHAT_MESSAGE",
      ReferenceId = bookingId,
      SenderName = senderName,
      Message = messagePreview
    });
  }
}
