using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <inheritdoc/>
public class NotificationRepository : INotificationRepository
{
  private readonly AppDbContext _db;

  public NotificationRepository(AppDbContext db)
  {
    _db = db;
  }

  /// <inheritdoc/>
  public async Task AddAsync(Notification notification)
  {
    _db.Notifications.Add(notification);
    await _db.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task<IList<Notification>> GetByUserIdAsync(Guid userId)
  {
    return await _db.Notifications
      .Where(n => n.UserId == userId)
      .OrderByDescending(n => n.CreatedAt)
      .Take(50) // Cap at 50 latest
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<int> GetUnreadCountAsync(Guid userId)
  {
    return await _db.Notifications
      .CountAsync(n => n.UserId == userId && !n.IsRead);
  }

  /// <inheritdoc/>
  public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
  {
    var notification = await _db.Notifications
      .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

    if (notification == null) return;

    notification.IsRead = true;
    await _db.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task MarkAllAsReadAsync(Guid userId)
  {
    var unread = await _db.Notifications
      .Where(n => n.UserId == userId && !n.IsRead)
      .ToListAsync();

    unread.ForEach(n => n.IsRead = true);
    await _db.SaveChangesAsync();
  }
}
