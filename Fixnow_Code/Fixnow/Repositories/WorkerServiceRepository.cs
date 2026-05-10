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
    var existing = await _context.WorkerServices
      .Where(ws => ws.WorkerId == workerId)
      .ToListAsync();
    
    var existingServiceIds = existing.Select(e => e.ServiceId).ToList();

    // Remove services that are no longer requested
    var toRemove = existing.Where(e => !serviceIds.Contains(e.ServiceId)).ToList();
    _context.WorkerServices.RemoveRange(toRemove);

    // Update existing ones that were REJECTED back to PENDING if still selected
    var toReSubmit = existing
      .Where(e => serviceIds.Contains(e.ServiceId) && e.Status == Enums.WorkerServiceStatus.REJECTED)
      .ToList();
    
    foreach (var ws in toReSubmit)
    {
      ws.Status = Enums.WorkerServiceStatus.PENDING;
    }

    // Add new ones with PENDING status
    var toAddIds = serviceIds.Except(existingServiceIds).ToList();
    var newServices = toAddIds.Select(id => new WorkerService
    {
      WorkerId = workerId,
      ServiceId = id,
      Status = Enums.WorkerServiceStatus.PENDING
    });

    _context.WorkerServices.AddRange(newServices);
    await _context.SaveChangesAsync();
  }

  public async Task<List<WorkerService>> GetPendingServicesAsync()
  {
    return await _context.WorkerServices
      .Include(ws => ws.Worker)
      .Include(ws => ws.Service)
      .Where(ws => ws.Status == Enums.WorkerServiceStatus.PENDING)
      .ToListAsync();
  }

  public async Task UpdateServiceStatusAsync(Guid workerId, Guid serviceId, Enums.WorkerServiceStatus status)
  {
    var workerService = await _context.WorkerServices
      .FirstOrDefaultAsync(ws => ws.WorkerId == workerId && ws.ServiceId == serviceId);
      
    if (workerService != null)
    {
      workerService.Status = status;
      await _context.SaveChangesAsync();
    }
  }
}
