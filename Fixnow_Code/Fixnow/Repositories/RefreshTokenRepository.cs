using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IRefreshTokenRepository.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
  private readonly AppDbContext _context;

  public RefreshTokenRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<RefreshToken?> FindByTokenAsync(string token)
  {
    return await _context.RefreshTokens
      .Include(r => r.User)
      .FirstOrDefaultAsync(r => r.Token == token && !r.Revoked);
  }

  /// <inheritdoc/>
  public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
  {
    _context.RefreshTokens.Add(refreshToken);
    await _context.SaveChangesAsync();
    return refreshToken;
  }

  /// <inheritdoc/>
  public async Task RevokeAsync(RefreshToken refreshToken)
  {
    refreshToken.Revoked = true;
    await _context.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task RevokeAllForUserAsync(Guid userId)
  {
    var tokens = await _context.RefreshTokens
      .Where(r => r.UserId == userId && !r.Revoked)
      .ToListAsync();

    tokens.ForEach(t => t.Revoked = true);
    await _context.SaveChangesAsync();
  }
}
