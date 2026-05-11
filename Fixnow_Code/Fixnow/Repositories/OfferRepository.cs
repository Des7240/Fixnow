using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class OfferRepository : IOfferRepository
{
  private readonly AppDbContext _context;

  public OfferRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<WorkerOffer?> GetByIdAsync(Guid id)
  {
    return await _context.WorkerOffers
      .Include(wo => wo.Worker)
      .Include(wo => wo.Attachments).ThenInclude(a => a.File)
      .FirstOrDefaultAsync(wo => wo.Id == id);
  }

  public async Task<IEnumerable<WorkerOffer>> GetByOpenJobIdAsync(Guid openJobId)
  {
    return await _context.WorkerOffers
      .Include(wo => wo.Worker)
      .Where(wo => wo.OpenJobId == openJobId)
      .OrderByDescending(wo => wo.CreatedAt)
      .ToListAsync();
  }

  public async Task<IEnumerable<WorkerOffer>> GetByWorkerIdAsync(Guid workerId)
  {
    return await _context.WorkerOffers
      .Include(wo => wo.OpenJob)
      .Where(wo => wo.WorkerId == workerId)
      .OrderByDescending(wo => wo.CreatedAt)
      .ToListAsync();
  }

  public async Task AddAsync(WorkerOffer offer)
  {
    await _context.WorkerOffers.AddAsync(offer);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(WorkerOffer offer)
  {
    _context.WorkerOffers.Update(offer);
    await _context.SaveChangesAsync();
  }
}
