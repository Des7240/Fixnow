using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class WorkerProfileRepository : IWorkerProfileRepository
{
  private readonly AppDbContext _context;

  public WorkerProfileRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<WorkerProfile?> FindByWorkerIdAsync(Guid workerId)
  {
    return await _context.WorkerProfiles
      .Include(p => p.User)
      .FirstOrDefaultAsync(p => p.UserId == workerId);
  }

  public async Task<WorkerProfile> CreateAsync(WorkerProfile profile)
  {
    _context.WorkerProfiles.Add(profile);
    await _context.SaveChangesAsync();
    return profile;
  }

  public async Task UpdateAsync(WorkerProfile profile)
  {
    _context.WorkerProfiles.Update(profile);
    await _context.SaveChangesAsync();
  }
}
