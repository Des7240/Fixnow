using Hangfire.Dashboard;

namespace Fixnow.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
  public bool Authorize(DashboardContext context)
  {
    // For MVP, allow all local or authenticated users, or just everyone in dev mode.
    // In production, we should check if the user has Admin role.
    
    // To allow everyone (for easy testing in dev environment):
    return true;

    // Example of production role check:
    // var httpContext = context.GetHttpContext();
    // return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("ADMIN");
  }
}
