using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Fixnow.Repositories;

public class OpenJobRepository : IOpenJobRepository
{
  private readonly AppDbContext _context;
  private static readonly GeometryFactory GeomFactory = new(new PrecisionModel(), 4326);

  public OpenJobRepository(AppDbContext context)
  {
    _context = context;
  }

  public async Task<OpenJob?> GetByIdAsync(Guid id)
  {
    return await _context.OpenJobs
      .Include(oj => oj.Customer)
      .Include(oj => oj.Service)
      .Include(oj => oj.Attachments).ThenInclude(a => a.File)
      .FirstOrDefaultAsync(oj => oj.Id == id);
  }

  public async Task<IEnumerable<OpenJob>> GetByCustomerIdAsync(Guid customerId)
  {
    return await _context.OpenJobs
      .Include(oj => oj.Service)
      .Where(oj => oj.CustomerId == customerId)
      .OrderByDescending(oj => oj.CreatedAt)
      .ToListAsync();
  }

  public async Task<IEnumerable<OpenJob>> GetNearbyJobsAsync(double lat, double lng, List<Guid> serviceIds)
  {
    var workerLocation = GeomFactory.CreatePoint(new Coordinate(lng, lat));

    return await _context.OpenJobs
      .Include(oj => oj.Customer)
      .Include(oj => oj.Service)
      .Where(oj => oj.Status == OpenJobStatus.OPEN || oj.Status == OpenJobStatus.RECEIVING_OFFERS)
      .Where(oj => serviceIds.Contains(oj.ServiceId))
      .Where(oj => oj.Location.IsWithinDistance(workerLocation, oj.RadiusKm * 1000))
      .OrderByDescending(oj => oj.CreatedAt)
      .ToListAsync();
  }

  public async Task AddAsync(OpenJob openJob)
  {
    await _context.OpenJobs.AddAsync(openJob);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateAsync(OpenJob openJob)
  {
    _context.OpenJobs.Update(openJob);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteAsync(OpenJob openJob)
  {
    _context.OpenJobs.Remove(openJob);
    await _context.SaveChangesAsync();
  }
}
