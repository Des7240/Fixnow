using Fixnow.Entities;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// </summary>
public interface IJwtService
{
  /// <summary>Generates a signed JWT access token for the given user.</summary>
  string GenerateAccessToken(User user);

  /// <summary>Generates a secure random refresh token string.</summary>
  string GenerateRefreshTokenValue();

  /// <summary>Returns the configured access token expiry in seconds.</summary>
  int GetAccessTokenExpirySeconds();
}
