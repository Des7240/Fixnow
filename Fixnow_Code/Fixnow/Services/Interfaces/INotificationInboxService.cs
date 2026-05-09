using Fixnow.DTOs.Notification;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service for managing in-app notification inbox.
/// </summary>
public interface INotificationInboxService
{
  /// <summary>Gets all notifications for the current user.</summary>
  Task<IList<NotificationDto>> GetMyNotificationsAsync(Guid userId);

  /// <summary>Gets unread notification count for the current user.</summary>
  Task<int> GetUnreadCountAsync(Guid userId);

  /// <summary>Marks a specific notification as read.</summary>
  Task MarkAsReadAsync(Guid notificationId, Guid userId);

  /// <summary>Marks all notifications as read for the current user.</summary>
  Task MarkAllAsReadAsync(Guid userId);
}
