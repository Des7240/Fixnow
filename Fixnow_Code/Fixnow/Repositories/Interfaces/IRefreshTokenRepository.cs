using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for RefreshToken entity operations.
/// </summary>
public interface IRefreshTokenRepository
{
  Task<RefreshToken?> FindByTokenAsync(string token);
  Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
  Task RevokeAsync(RefreshToken refreshToken);
  Task RevokeAllForUserAsync(Guid userId);
}
