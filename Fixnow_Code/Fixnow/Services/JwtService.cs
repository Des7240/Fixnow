using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Fixnow.Entities;
using Fixnow.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Fixnow.Services;

/// <summary>
/// Implementation of IJwtService using System.IdentityModel.Tokens.Jwt.
/// </summary>
public class JwtService : IJwtService
{
  private readonly IConfiguration _config;

  public JwtService(IConfiguration config)
  {
    _config = config;
  }

  /// <inheritdoc/>
  public string GenerateAccessToken(User user)
  {
    var secretKey = _config["Jwt:SecretKey"]
      ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Email, user.Email),
      new(ClaimTypes.Name, user.FullName),
      new(ClaimTypes.Role, user.Role.ToString()),
      new("avatar", user.AvatarUrl ?? string.Empty),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    };

    var expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");

    var token = new JwtSecurityToken(
      issuer: _config["Jwt:Issuer"],
      audience: _config["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  /// <inheritdoc/>
  public string GenerateRefreshTokenValue()
  {
    var randomBytes = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
  }

  /// <inheritdoc/>
  public int GetAccessTokenExpirySeconds()
  {
    var minutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
    return minutes * 60;
  }
}
