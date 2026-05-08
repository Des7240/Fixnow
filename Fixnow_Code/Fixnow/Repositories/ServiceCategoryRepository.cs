using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IServiceCategoryRepository.
/// </summary>
public class ServiceCategoryRepository : IServiceCategoryRepository
{
  private readonly AppDbContext _context;

  public ServiceCategoryRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<List<ServiceCategory>> FindAllActiveAsync()
  {
    return await _context.ServiceCategories
      .Where(s => s.IsActive)
      .OrderBy(s => s.Name)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<ServiceCategory?> FindByIdAsync(Guid id)
  {
    return await _context.ServiceCategories.FindAsync(id);
  }

  /// <inheritdoc/>
  public async Task<ServiceCategory> CreateAsync(ServiceCategory service)
  {
    _context.ServiceCategories.Add(service);
    await _context.SaveChangesAsync();
    return service;
  }
}
