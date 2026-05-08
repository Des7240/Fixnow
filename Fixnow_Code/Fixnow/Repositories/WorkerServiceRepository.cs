using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class WorkerServiceRepository : IWorkerServiceRepository
{
  private readonly AppDbContext _context;

  public WorkerServiceRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<List<WorkerService>> FindByWorkerIdAsync(Guid workerId)
  {
    return await _context.WorkerServices
      .Include(ws => ws.Service)
      .Where(ws => ws.WorkerId == workerId)
      .ToListAsync();
  }

  public async Task UpdateWorkerServicesAsync(Guid workerId, List<Guid> serviceIds)
  {
    // Remove existing services
    var existing = await _context.WorkerServices
      .Where(ws => ws.WorkerId == workerId)
      .ToListAsync();
    
    _context.WorkerServices.RemoveRange(existing);

    // Add new ones
    var newServices = serviceIds.Select(id => new WorkerService
    {
      WorkerId = workerId,
      ServiceId = id
    });

    _context.WorkerServices.AddRange(newServices);
    await _context.SaveChangesAsync();
  }
}
