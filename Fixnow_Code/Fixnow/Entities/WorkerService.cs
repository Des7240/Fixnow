namespace Fixnow.Entities;

/// <summary>
/// Many-to-many relationship mapping between a Worker and ServiceCategories (Skills).
/// </summary>
public class WorkerService
{
  public Guid WorkerId { get; set; }
  public Guid ServiceId { get; set; }

  // Navigation
  public User Worker { get; set; } = null!;
  public ServiceCategory Service { get; set; } = null!;
}
