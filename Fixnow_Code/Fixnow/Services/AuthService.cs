using Fixnow.DTOs.Auth;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <summary>
/// Implementation of IAuthService handling register, login, refresh, and logout.
/// </summary>
public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepo;
  private readonly IRefreshTokenRepository _refreshTokenRepo;
  private readonly IJwtService _jwtService;
  private readonly IConfiguration _config;
  private readonly IAuditService _auditService;

  public AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtService jwtService,
    IConfiguration config,
    IAuditService auditService)
  {
    _userRepo = userRepo;
    _refreshTokenRepo = refreshTokenRepo;
    _jwtService = jwtService;
    _config = config;
    _auditService = auditService;
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
        Role = user.Role.ToString(),
      }
    };
  }
}
