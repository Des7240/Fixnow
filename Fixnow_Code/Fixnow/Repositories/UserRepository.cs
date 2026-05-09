using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
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
    return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
  }

  /// <inheritdoc/>
  public async Task<User?> FindByIdAsync(Guid id)
  {
    return await _context.Users
      .Include(u => u.WorkerProfile)
      .FirstOrDefaultAsync(u => u.Id == id);
  }

  /// <inheritdoc/>
  public async Task<List<User>> GetByRoleAsync(UserRole role)
  {
    return await _context.Users
      .Include(u => u.WorkerProfile)
      .Where(u => u.Role == role)
      .ToListAsync();
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

  /// <inheritdoc/>
  public async Task UpdateAsync(User user)
  {
    user.UpdatedAt = DateTime.UtcNow;
    _context.Users.Update(user);
    await _context.SaveChangesAsync();
  }
}

