namespace Fixnow.Middlewares;

public class SecurityHeadersMiddleware
{
  private readonly RequestDelegate _next;

  public SecurityHeadersMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    // Chống Clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    // Chống MIME-sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    // Bắt buộc dùng HTTPS
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    // Cho phép Google Login pop-up hoạt động với COOP
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    // Giảm thiểu rò rỉ thông tin qua Referrer
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    // Tắt tự động phát hiện XSS (Dùng CSP thay thế, nhưng thêm vào cho an toàn cơ bản)
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    await _next(context);
  }
}
