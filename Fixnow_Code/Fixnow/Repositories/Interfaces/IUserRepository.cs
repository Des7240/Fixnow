using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository
{
  Task<User?> FindByEmailAsync(string email);
  Task<User?> FindByIdAsync(Guid id);
  Task<bool> ExistsByEmailAsync(string email);
  Task<User> CreateAsync(User user);
  Task UpdateAsync(User user);
}
