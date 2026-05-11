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

  public async Task<IEnumerable<OpenJob>> GetMarketplaceJobsAsync(
    double lat,
    double lng,
    double radiusKm,
    List<Guid>? serviceIds,
    decimal? minBudget,
    decimal? maxBudget,
    string? urgencyLevel,
    string? sortBy)
  {
    var workerLocation = GeomFactory.CreatePoint(new Coordinate(lng, lat));

    var query = _context.OpenJobs
      .Include(oj => oj.Customer)
      .Include(oj => oj.Service)
      .Include(oj => oj.Offers)
      .Where(oj => oj.Status == OpenJobStatus.OPEN || oj.Status == OpenJobStatus.RECEIVING_OFFERS)
      .Where(oj => oj.Location.IsWithinDistance(workerLocation, radiusKm * 1000));

    if (serviceIds != null && serviceIds.Any())
    {
      query = query.Where(oj => serviceIds.Contains(oj.ServiceId));
    }

    if (minBudget.HasValue)
    {
      query = query.Where(oj => oj.MinBudget >= minBudget.Value || oj.MaxBudget >= minBudget.Value);
    }

    if (maxBudget.HasValue)
    {
      query = query.Where(oj => oj.MaxBudget <= maxBudget.Value || oj.MinBudget <= maxBudget.Value);
    }

    if (!string.IsNullOrEmpty(urgencyLevel))
    {
      query = query.Where(oj => oj.UrgencyLevel == urgencyLevel);
    }

    // Apply sorting
    query = sortBy?.ToLower() switch
    {
      "nearest" => query.OrderBy(oj => oj.Location.Distance(workerLocation)),
      "highest_budget" => query.OrderByDescending(oj => oj.MaxBudget ?? oj.MinBudget ?? 0),
      "latest" => query.OrderByDescending(oj => oj.CreatedAt),
      "least_offers" => query.OrderBy(oj => oj.Offers.Count),
      "urgent" => query.OrderByDescending(oj => oj.UrgencyLevel == "URGENT").ThenByDescending(oj => oj.CreatedAt),
      _ => query.OrderByDescending(oj => oj.CreatedAt)
    };

    return await query.ToListAsync();
  }

  public async Task SaveJobAsync(SavedOpenJob savedJob)
  {
    var exists = await _context.SavedOpenJobs
      .AnyAsync(s => s.WorkerId == savedJob.WorkerId && s.OpenJobId == savedJob.OpenJobId);
    
    if (!exists)
    {
      await _context.SavedOpenJobs.AddAsync(savedJob);
      await _context.SaveChangesAsync();
    }
  }

  public async Task UnsaveJobAsync(Guid workerId, Guid jobId)
  {
    var savedJob = await _context.SavedOpenJobs
      .FirstOrDefaultAsync(s => s.WorkerId == workerId && s.OpenJobId == jobId);
    
    if (savedJob != null)
    {
      _context.SavedOpenJobs.Remove(savedJob);
      await _context.SaveChangesAsync();
    }
  }

  public async Task<IEnumerable<OpenJob>> GetSavedJobsAsync(Guid workerId)
  {
    return await _context.SavedOpenJobs
      .Where(s => s.WorkerId == workerId)
      .Include(s => s.OpenJob).ThenInclude(oj => oj.Customer)
      .Include(s => s.OpenJob).ThenInclude(oj => oj.Service)
      .Select(s => s.OpenJob)
      .ToListAsync();
  }

  public async Task<bool> IsJobSavedAsync(Guid workerId, Guid jobId)
  {
    return await _context.SavedOpenJobs
      .AnyAsync(s => s.WorkerId == workerId && s.OpenJobId == jobId);
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

  public async Task<IEnumerable<OpenJob>> GetAllForAdminAsync()
  {
    return await _context.OpenJobs
      .Include(oj => oj.Customer)
      .Include(oj => oj.Service)
      .Include(oj => oj.Offers)
      .OrderByDescending(oj => oj.CreatedAt)
      .ToListAsync();
  }
}
