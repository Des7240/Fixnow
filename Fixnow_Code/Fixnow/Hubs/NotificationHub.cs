using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Fixnow.Hubs;

[Authorize]
public class NotificationHub : Hub
{
  public override async Task OnConnectedAsync()
  {
    await base.OnConnectedAsync();
  }
}
