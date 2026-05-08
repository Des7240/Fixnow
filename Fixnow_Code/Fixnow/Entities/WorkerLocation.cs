using NetTopologySuite.Geometries;

namespace Fixnow.Entities;

/// <summary>
/// Tracks a worker's current GPS location. One row per worker (upsert pattern).
/// </summary>
public class WorkerLocation
{
  public Guid WorkerId { get; set; }

  /// <summary>PostGIS geography(Point, 4326) – SRID 4326 (WGS84).</summary>
  public Point Location { get; set; } = null!;

  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User Worker { get; set; } = null!;
}
