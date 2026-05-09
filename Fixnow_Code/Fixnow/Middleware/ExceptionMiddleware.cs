using System.Net;
using System.Text.Json;

namespace Fixnow.Middleware;

/// <summary>
/// Global exception handling middleware that returns standardized error responses.
/// </summary>
public class ExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionMiddleware> _logger;

  public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  /// <summary>Processes the HTTP request and catches unhandled exceptions.</summary>
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
      await HandleExceptionAsync(context, ex);
    }
  }

  private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var (statusCode, message) = exception switch
    {
      InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
      UnauthorizedAccessException => (HttpStatusCode.Unauthorized, exception.Message),
      KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
      _ => (HttpStatusCode.InternalServerError, exception.ToString())
    };

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)statusCode;

    var errorResponse = new
    {
      statusCode = (int)statusCode,
      message,
      timestamp = DateTime.UtcNow,
    };

    var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await context.Response.WriteAsync(json);
  }
}
