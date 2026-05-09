using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository for managing in-app notifications.
/// </summary>
public interface INotificationRepository
{
  /// <summary>Creates a new notification record.</summary>
  Task AddAsync(Notification notification);

  /// <summary>Gets all notifications for a user, newest first.</summary>
  Task<IList<Notification>> GetByUserIdAsync(Guid userId);

  /// <summary>Gets unread count for a user.</summary>
  Task<int> GetUnreadCountAsync(Guid userId);

  /// <summary>Marks a specific notification as read.</summary>
  Task MarkAsReadAsync(Guid notificationId, Guid userId);

  /// <summary>Marks all notifications for a user as read.</summary>
  Task MarkAllAsReadAsync(Guid userId);
}
