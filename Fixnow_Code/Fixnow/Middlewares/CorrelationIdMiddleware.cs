using Serilog.Context;

namespace Fixnow.Middlewares;

public class CorrelationIdMiddleware
{
  private readonly RequestDelegate _next;
  private const string CorrelationIdHeaderName = "X-Correlation-ID";

  public CorrelationIdMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var correlationId = GetCorrelationId(context);

    // Push the correlation ID to Serilog's LogContext
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
      await _next(context);
    }
  }

  private string GetCorrelationId(HttpContext context)
  {
    if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
    {
      return correlationId.ToString();
    }

    var newCorrelationId = Guid.NewGuid().ToString("N");
    // Optionally add it to response headers
    context.Response.Headers.Append(CorrelationIdHeaderName, newCorrelationId);

    return newCorrelationId;
  }
}
