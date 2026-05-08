using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class WorkerKycRepository : IWorkerKycRepository
{
  private readonly AppDbContext _context;

  public WorkerKycRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<WorkerKyc?> FindByIdAsync(Guid id)
  {
    return await _context.WorkerKycs
      .Include(k => k.Worker)
      .FirstOrDefaultAsync(k => k.Id == id);
  }

  public async Task<WorkerKyc?> FindLatestByWorkerIdAsync(Guid workerId)
  {
    return await _context.WorkerKycs
      .Where(k => k.WorkerId == workerId)
      .OrderByDescending(k => k.SubmittedAt)
      .FirstOrDefaultAsync();
  }

  public async Task<WorkerKyc> CreateAsync(WorkerKyc kyc)
  {
    _context.WorkerKycs.Add(kyc);
    await _context.SaveChangesAsync();
    return kyc;
  }

  public async Task UpdateAsync(WorkerKyc kyc)
  {
    _context.WorkerKycs.Update(kyc);
    await _context.SaveChangesAsync();
  }
}
