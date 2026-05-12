namespace Fixnow.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendOtpEmailAsync(string to, string otp, string purpose);
}
