using Fixnow.Enums;
using NetTopologySuite.Geometries;

namespace Fixnow.Entities;

/// <summary>
/// Represents a service booking order created by a customer.
/// </summary>
public class Booking
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid CustomerId { get; set; }
  public Guid? WorkerId { get; set; }
  public Guid ServiceId { get; set; }
  public BookingStatus Status { get; set; } = BookingStatus.PENDING;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }

  /// <summary>PostGIS geography(Point, 4326) for geo queries.</summary>
  public Point Location { get; set; } = null!;

  public string? Description { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User Customer { get; set; } = null!;
  public User? Worker { get; set; }
  public ServiceCategory Service { get; set; } = null!;
  public ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();
  public ICollection<BookingMatchingLog> MatchingLogs { get; set; } = new List<BookingMatchingLog>();
}
