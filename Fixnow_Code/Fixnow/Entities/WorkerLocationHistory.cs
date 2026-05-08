using NetTopologySuite.Geometries;

namespace Fixnow.Entities;

/// <summary>
/// History of a worker's GPS locations.
/// </summary>
public class WorkerLocationHistory
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid WorkerId { get; set; }
  public Point Location { get; set; } = null!;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User Worker { get; set; } = null!;
}
