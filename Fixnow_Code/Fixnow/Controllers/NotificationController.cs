using System.Security.Claims;
using Fixnow.DTOs.Notification;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// API for the user's in-app notification inbox.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
  private readonly INotificationInboxService _inboxService;

  public NotificationController(INotificationInboxService inboxService)
  {
    _inboxService = inboxService;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  /// <summary>GET /api/v1/notifications — get all notifications.</summary>
  [HttpGet]
  public async Task<ActionResult<IList<NotificationDto>>> GetMyNotifications()
  {
    var result = await _inboxService.GetMyNotificationsAsync(CurrentUserId);
    return Ok(result);
  }

  /// <summary>GET /api/v1/notifications/unread-count — badge count.</summary>
  [HttpGet("unread-count")]
  public async Task<ActionResult<int>> GetUnreadCount()
  {
    var count = await _inboxService.GetUnreadCountAsync(CurrentUserId);
    return Ok(count);
  }

  /// <summary>PATCH /api/v1/notifications/{id}/read — mark one as read.</summary>
  [HttpPatch("{id:guid}/read")]
  public async Task<IActionResult> MarkAsRead(Guid id)
  {
    await _inboxService.MarkAsReadAsync(id, CurrentUserId);
    return NoContent();
  }

  /// <summary>PATCH /api/v1/notifications/read-all — mark all as read.</summary>
  [HttpPatch("read-all")]
  public async Task<IActionResult> MarkAllAsRead()
  {
    await _inboxService.MarkAllAsReadAsync(CurrentUserId);
    return NoContent();
  }
}
