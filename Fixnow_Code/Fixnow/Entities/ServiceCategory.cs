namespace Fixnow.Entities;

/// <summary>
/// Represents a type of service offered on the FixNow platform.
/// E.g: Electrical, Plumbing, Air Conditioner, etc.
/// </summary>
public class ServiceCategory
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? IconUrl { get; set; }
  public decimal BasePrice { get; set; } = 0;
  public int EstimatedDurationMinutes { get; set; } = 60;
  public bool IsActive { get; set; } = true;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
