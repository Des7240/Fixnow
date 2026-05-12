using Fixnow.Entities;

namespace Fixnow.Services.Interfaces;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, OtpType type, string purpose);
    Task<bool> VerifyOtpAsync(string email, string code, OtpType type, bool markAsUsed = true);
}
