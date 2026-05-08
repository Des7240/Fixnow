using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
  private readonly AppDbContext _context;

  public UserRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<User?> FindByEmailAsync(string email)
  {
    return await _context.Users
      .FirstOrDefaultAsync(u => u.Email == email);
  }

  /// <inheritdoc/>
  public async Task<User?> FindByIdAsync(Guid id)
  {
    return await _context.Users.FindAsync(id);
  }

  /// <inheritdoc/>
  public async Task<bool> ExistsByEmailAsync(string email)
  {
    return await _context.Users.AnyAsync(u => u.Email == email);
  }

  /// <inheritdoc/>
  public async Task<User> CreateAsync(User user)
  {
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return user;
  }
}
