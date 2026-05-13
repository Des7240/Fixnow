using Fixnow.DTOs.Auth;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Google.Apis.Auth;

namespace Fixnow.Services;

/// <summary>
/// Implementation of IAuthService handling register, login, refresh, logout, and password reset.
/// </summary>
public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepo;
  private readonly IRefreshTokenRepository _refreshTokenRepo;
  private readonly IJwtService _jwtService;
  private readonly IConfiguration _config;
  private readonly IAuditService _auditService;
  private readonly IOtpService _otpService;
  private readonly ILogger<AuthService> _logger;

  public AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtService jwtService,
    IConfiguration config,
    IAuditService auditService,
    IOtpService otpService,
    ILogger<AuthService> logger)
  {
    _userRepo = userRepo;
    _refreshTokenRepo = refreshTokenRepo;
    _jwtService = jwtService;
    _config = config;
    _auditService = auditService;
    _otpService = otpService;
    _logger = logger;
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
  {
    var exists = await _userRepo.ExistsByEmailAsync(request.Email);
    if (exists)
      throw new InvalidOperationException("Email already registered.");

    var user = new User
    {
      Email = request.Email,
      FullName = request.FullName,
      Role = request.Role,
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
    };

    await _userRepo.CreateAsync(user);

    await _auditService.LogActionAsync("REGISTER_SUCCESS", "User", user.Id, user.Role.ToString(), user.Id, null, null);

    return await BuildAuthResponseAsync(user);
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    var user = await _userRepo.FindByEmailAsync(request.Email)
      ?? throw new UnauthorizedAccessException("Invalid email or password.");

    var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
    if (!isValid)
    {
      await _auditService.LogActionAsync("LOGIN_FAILED", "User", null, null, null, null, $"{{ \"email\": \"{request.Email}\" }}");
      throw new UnauthorizedAccessException("Invalid email or password.");
    }

    await _auditService.LogActionAsync("LOGIN_SUCCESS", "User", user.Id, user.Role.ToString(), user.Id, null, null);

    return await BuildAuthResponseAsync(user);
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
  {
    var clientId = _config["Google:ClientId"];
    var settings = new GoogleJsonWebSignature.ValidationSettings
    {
        Audience = new[] { clientId }
    };

    GoogleJsonWebSignature.Payload payload;
    try
    {
        payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Invalid Google ID Token");
        throw new UnauthorizedAccessException("Mã xác thực Google không hợp lệ.");
    }

    var user = await _userRepo.FindByEmailAsync(payload.Email);
    if (user == null)
    {
        // Auto-register new Google user
        user = new User
        {
            Email = payload.Email,
            EmailVerified = true, // Google already verified email
            FullName = payload.Name,
            AvatarUrl = payload.Picture,
            GoogleId = payload.Subject,
            AuthProvider = AuthProvider.GOOGLE,
            Role = UserRole.CUSTOMER, // Default role
            PasswordHash = string.Empty, // No password for Google users
            NeedsPasswordReset = true // Force password set on first login
        };
        await _userRepo.CreateAsync(user);

        await _auditService.LogActionAsync("REGISTER_SUCCESS", "User", user.Id, user.Role.ToString(), user.Id, null, "{ \"provider\": \"GOOGLE\" }");
    }
    else
    {
        // Update Google ID if not present
        if (string.IsNullOrEmpty(user.GoogleId))
        {
            user.GoogleId = payload.Subject;
            user.AuthProvider = AuthProvider.GOOGLE;
            user.EmailVerified = true;
            await _userRepo.UpdateAsync(user);
        }
    }

    await _auditService.LogActionAsync("LOGIN_SUCCESS", "User", user.Id, user.Role.ToString(), user.Id, null, "{ \"provider\": \"GOOGLE\" }");

    return await BuildAuthResponseAsync(user);
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
  {
    var token = await _refreshTokenRepo.FindByTokenAsync(refreshToken)
      ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

    if (token.ExpiresAt < DateTime.UtcNow)
    {
      await _refreshTokenRepo.RevokeAsync(token);
      throw new UnauthorizedAccessException("Refresh token expired.");
    }

    await _refreshTokenRepo.RevokeAsync(token);
    return await BuildAuthResponseAsync(token.User);
  }

  /// <inheritdoc/>
  public async Task LogoutAsync(string refreshToken)
  {
    var token = await _refreshTokenRepo.FindByTokenAsync(refreshToken);
    if (token is not null)
      await _refreshTokenRepo.RevokeAsync(token);
  }

  /// <inheritdoc/>
  public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
  {
    var user = await _userRepo.FindByIdAsync(userId)
      ?? throw new KeyNotFoundException("User not found.");

    // If user has a password, verify old password. 
    // If not (e.g. Google user first login), allow setting new password directly.
    if (!string.IsNullOrEmpty(user.PasswordHash))
    {
        var isValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
        if (!isValid)
        {
          await _auditService.LogActionAsync("CHANGE_PASSWORD_FAILED", "User", userId, user.Role.ToString(), userId, null, "{ \"reason\": \"Invalid old password\" }");
          throw new UnauthorizedAccessException("Mật khẩu cũ không chính xác.");
        }
    }

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.NeedsPasswordReset = false;
    await _userRepo.UpdateAsync(user);

    await _auditService.LogActionAsync("CHANGE_PASSWORD_SUCCESS", "User", userId, user.Role.ToString(), userId, null, null);
  }

  /// <inheritdoc/>
  public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
  {
    var user = await _userRepo.FindByEmailAsync(request.Email);
    if (user == null)
    {
      return;
    }

    await _otpService.GenerateOtpAsync(request.Email, OtpType.PASSWORD_RESET, "Khôi phục mật khẩu");
    await _auditService.LogActionAsync("FORGOT_PASSWORD_REQUESTED", "User", user.Id, user.Role.ToString(), null, null, $"{{ \"email\": \"{request.Email}\" }}");
  }

  /// <inheritdoc/>
  public async Task<bool> VerifyResetOtpAsync(VerifyOtpRequestDto request)
  {
    return await _otpService.VerifyOtpAsync(request.Email, request.Code, OtpType.PASSWORD_RESET, markAsUsed: false);
  }

  /// <inheritdoc/>
  public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
  {
    var isValid = await _otpService.VerifyOtpAsync(request.Email, request.Code, OtpType.PASSWORD_RESET);
    if (!isValid)
    {
      throw new InvalidOperationException("Mã xác thực không chính xác hoặc đã hết hạn.");
    }

    var user = await _userRepo.FindByEmailAsync(request.Email)
      ?? throw new KeyNotFoundException("User not found.");

    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    user.NeedsPasswordReset = false;
    await _userRepo.UpdateAsync(user);

    // Revoke all refresh tokens for safety
    await _refreshTokenRepo.RevokeAllForUserAsync(user.Id);

    await _auditService.LogActionAsync("PASSWORD_RESET_SUCCESS", "User", user.Id, user.Role.ToString(), user.Id, null, null);
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
  {
    var user = await _userRepo.FindByIdAsync(userId)
      ?? throw new KeyNotFoundException("User not found.");

    user.FullName = request.FullName;
    user.PhoneNumber = request.PhoneNumber ?? string.Empty;
    if (!string.IsNullOrEmpty(request.AvatarUrl))
    {
        user.AvatarUrl = request.AvatarUrl;
    }
    user.UpdatedAt = DateTime.UtcNow;

    await _userRepo.UpdateAsync(user);

    await _auditService.LogActionAsync("UPDATE_PROFILE_SUCCESS", "User", userId, user.Role.ToString(), userId, null, null);

    return await BuildAuthResponseAsync(user);
  }

  /// <summary>Builds a full auth response with new access + refresh tokens.</summary>
  private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
  {
    var accessToken = _jwtService.GenerateAccessToken(user);
    var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();
    var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

    var refreshToken = new RefreshToken
    {
      UserId = user.Id,
      Token = refreshTokenValue,
      ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
    };

    await _refreshTokenRepo.CreateAsync(refreshToken);

    return new AuthResponseDto
    {
      AccessToken = accessToken,
      RefreshToken = refreshTokenValue,
      ExpiresIn = _jwtService.GetAccessTokenExpirySeconds(),
      User = new UserInfoDto
      {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        NeedsPasswordReset = user.NeedsPasswordReset || string.IsNullOrEmpty(user.PasswordHash),
        AvatarUrl = user.AvatarUrl,
        Role = user.Role.ToString(),
      }
    };
  }
}
