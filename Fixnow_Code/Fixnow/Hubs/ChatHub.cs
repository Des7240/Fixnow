using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Fixnow.Hubs;

[Authorize]
public class ChatHub : Hub
{
  private readonly IConversationRepository _conversationRepo;

  public ChatHub(IConversationRepository conversationRepo)
  {
    _conversationRepo = conversationRepo;
  }

  private Guid CurrentUserId => Guid.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

  public override async Task OnConnectedAsync()
  {
    var userId = CurrentUserId;
    if (userId != Guid.Empty)
    {
      // Find all conversations of this user and add the connection to groups
      var conversations = await _conversationRepo.FindByUserAsync(userId);
      foreach (var conv in conversations)
      {
        await Groups.AddToGroupAsync(Context.ConnectionId, conv.Id.ToString());
      }
    }

    await base.OnConnectedAsync();
  }

  // Clients can call this if they need to explicitly join a new conversation's group
  public async Task JoinConversation(string conversationId)
  {
    if (Guid.TryParse(conversationId, out var convId))
    {
      var conversation = await _conversationRepo.FindByIdAsync(convId);
      if (conversation != null && (conversation.CustomerId == CurrentUserId || conversation.WorkerId == CurrentUserId))
      {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
      }
    }
  }
}
