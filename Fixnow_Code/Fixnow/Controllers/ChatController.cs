using Fixnow.DTOs.Chat;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/chat")]
[Authorize]
public class ChatController : ControllerBase
{
  private readonly IChatService _chatService;

  public ChatController(IChatService chatService)
  {
    _chatService = chatService;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  [HttpGet("conversations")]
  public async Task<IActionResult> GetConversations()
  {
    var result = await _chatService.GetConversationsAsync(CurrentUserId);
    return Ok(result);
  }

  [HttpGet("conversations/{id}/messages")]
  public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
  {
    var (items, totalCount) = await _chatService.GetMessagesAsync(id, CurrentUserId, page, pageSize);
    return Ok(new
    {
      TotalCount = totalCount,
      Page = page,
      PageSize = pageSize,
      Items = items
    });
  }

  [HttpPost("messages")]
  public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
  {
    var result = await _chatService.SendMessageAsync(request, CurrentUserId);
    return Ok(result);
  }

  [HttpPost("messages/read")]
  public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequestDto request)
  {
    await _chatService.MarkAsReadAsync(request.ConversationId, CurrentUserId);
    return Ok();
  }
}

public class MarkAsReadRequestDto
{
  public Guid ConversationId { get; set; }
}
