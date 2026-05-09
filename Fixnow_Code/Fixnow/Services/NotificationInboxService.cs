using Fixnow.DTOs.Notification;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <inheritdoc/>
public class NotificationInboxService : INotificationInboxService
{
  private readonly INotificationRepository _notificationRepo;

  public NotificationInboxService(INotificationRepository notificationRepo)
  {
    _notificationRepo = notificationRepo;
  }

  /// <inheritdoc/>
  public async Task<IList<NotificationDto>> GetMyNotificationsAsync(Guid userId)
  {
    var notifications = await _notificationRepo.GetByUserIdAsync(userId);
    return notifications.Select(n => new NotificationDto
    {
      Id = n.Id,
      Title = n.Title,
      Content = n.Content,
      Type = n.Type,
      IsRead = n.IsRead,
      ReferenceId = n.ReferenceId,
      CreatedAt = n.CreatedAt
    }).ToList();
  }

  /// <inheritdoc/>
  public Task<int> GetUnreadCountAsync(Guid userId)
  {
    return _notificationRepo.GetUnreadCountAsync(userId);
  }

  /// <inheritdoc/>
  public Task MarkAsReadAsync(Guid notificationId, Guid userId)
  {
    return _notificationRepo.MarkAsReadAsync(notificationId, userId);
  }

  /// <inheritdoc/>
  public Task MarkAllAsReadAsync(Guid userId)
  {
    return _notificationRepo.MarkAllAsReadAsync(userId);
  }
}
