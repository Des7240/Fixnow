using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for ServiceCategory CRUD operations.
/// </summary>
public interface IServiceCategoryRepository
{
  Task<List<ServiceCategory>> FindAllActiveAsync();
  Task<ServiceCategory?> FindByIdAsync(Guid id);
  Task<ServiceCategory> CreateAsync(ServiceCategory service);
}
