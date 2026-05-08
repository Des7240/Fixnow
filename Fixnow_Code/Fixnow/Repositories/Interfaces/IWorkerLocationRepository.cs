using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for worker location operations including PostGIS geo queries.
/// </summary>
public interface IWorkerLocationRepository
{
  Task UpsertAsync(Guid workerId, double lat, double lng);
  Task<WorkerLocation?> FindByWorkerIdAsync(Guid workerId);
  Task<List<NearbyWorkerResult>> FindNearbyAvailableWorkersAsync(
    double lat, double lng, double radiusMeters = 5000, int limit = 20);
}

/// <summary>Result model for nearby worker geo query.</summary>
public record NearbyWorkerResult(
  Guid WorkerId,
  string WorkerName,
  string WorkerEmail,
  double DistanceMeters
);
