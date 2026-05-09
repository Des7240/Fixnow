using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IWorkerLocationRepository using PostGIS geo queries.
/// </summary>
public class WorkerLocationRepository : IWorkerLocationRepository
{
  private readonly AppDbContext _context;
  private static readonly GeometryFactory GeomFactory = new(new PrecisionModel(), 4326);

  public WorkerLocationRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Upserts a worker's GPS location. Creates if not exists, updates if exists.
  /// </summary>
  public async Task UpsertAsync(Guid workerId, double lat, double lng)
  {
    var point = GeomFactory.CreatePoint(new Coordinate(lng, lat));
    var existing = await _context.WorkerLocations.FindAsync(workerId);

    if (existing is null)
    {
      _context.WorkerLocations.Add(new WorkerLocation
      {
        WorkerId = workerId,
        Location = point,
        UpdatedAt = DateTime.UtcNow,
      });
    }
    else
    {
      existing.Location = point;
      existing.UpdatedAt = DateTime.UtcNow;
    }

    // Save location history
    _context.WorkerLocationHistories.Add(new WorkerLocationHistory
    {
      WorkerId = workerId,
      Location = point,
      CreatedAt = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task<WorkerLocation?> FindByWorkerIdAsync(Guid workerId)
  {
    return await _context.WorkerLocations.FindAsync(workerId);
  }

  /// <summary>
  /// Finds available workers within radius using ST_DWithin + ST_Distance (PostGIS geography).
  /// Workers are excluded if they have an active booking or stale location (older than 1 hour).
  /// </summary>
  public async Task<List<NearbyWorkerResult>> FindNearbyAvailableWorkersAsync(
    double lat, double lng, Guid serviceId, double radiusMeters = 10000, int limit = 20)
  {
    var referencePoint = GeomFactory.CreatePoint(new Coordinate(lng, lat));
    var cutoff = DateTime.UtcNow.AddHours(-1);

    var activeStatuses = new[]
    {
      BookingStatus.ASSIGNED,
      BookingStatus.ON_THE_WAY,
      BookingStatus.WORKING,
    };

    // Workers who currently have an active booking
    var busyWorkerIds = await _context.Bookings
      .Where(b => activeStatuses.Contains(b.Status) && b.WorkerId.HasValue)
      .Select(b => b.WorkerId!.Value)
      .Distinct()
      .ToListAsync();

    return await _context.WorkerLocations
      .Include(wl => wl.Worker)
        .ThenInclude(u => u.WorkerProfile)
      .Include(wl => wl.Worker)
        .ThenInclude(u => u.WorkerServices)
      .Where(wl => wl.Worker.Role == UserRole.WORKER)
      .Where(wl => wl.Worker.Status == "ACTIVE")
      .Where(wl => wl.Worker.WorkerProfile != null && wl.Worker.WorkerProfile.AvailabilityStatus == WorkerAvailability.ONLINE)
      .Where(wl => wl.Worker.WorkerServices.Any(ws => ws.ServiceId == serviceId))
      .Where(wl => wl.UpdatedAt >= cutoff)
      .Where(wl => !busyWorkerIds.Contains(wl.WorkerId))
      .Where(wl => wl.Location.IsWithinDistance(referencePoint, radiusMeters))
      .OrderBy(wl => wl.Location.Distance(referencePoint))
      .Take(limit)
      .Select(wl => new NearbyWorkerResult(
        wl.WorkerId,
        wl.Worker.FullName,
        wl.Worker.Email,
        wl.Location.Distance(referencePoint)
      ))
      .ToListAsync();
  }
}
