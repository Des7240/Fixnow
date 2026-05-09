using Fixnow.Entities;
using Fixnow.Enums;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository
{
  Task<User?> FindByEmailAsync(string email);
  Task<User?> FindByIdAsync(Guid id);
  Task<List<User>> GetByRoleAsync(UserRole role);
  Task<bool> ExistsByEmailAsync(string email);
  Task<User> CreateAsync(User user);
  Task UpdateAsync(User user);
}
