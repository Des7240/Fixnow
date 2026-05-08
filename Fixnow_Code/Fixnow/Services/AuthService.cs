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

  public AuthService(
    IUserRepository userRepo,
    IRefreshTokenRepository refreshTokenRepo,
    IJwtService jwtService,
    IConfiguration config)
  {
    _userRepo = userRepo;
    _refreshTokenRepo = refreshTokenRepo;
    _jwtService = jwtService;
    _config = config;
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
    return await BuildAuthResponseAsync(user);
  }

  /// <inheritdoc/>
  public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    var user = await _userRepo.FindByEmailAsync(request.Email)
      ?? throw new UnauthorizedAccessException("Invalid email or password.");

    var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
    if (!isValid)
      throw new UnauthorizedAccessException("Invalid email or password.");

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
