using Fixnow.DTOs.Auth;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
  Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
  Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
  Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
  Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
  Task LogoutAsync(string refreshToken);
  Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
  Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
  Task<bool> VerifyResetOtpAsync(VerifyOtpRequestDto request);
  Task ResetPasswordAsync(ResetPasswordRequestDto request);
  Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
}
