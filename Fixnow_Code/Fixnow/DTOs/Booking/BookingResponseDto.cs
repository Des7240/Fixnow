namespace Fixnow.DTOs.Booking;

/// <summary>
/// Full booking response DTO including worker and service info.
/// </summary>
public class BookingResponseDto
{
  public Guid Id { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }
  public string? Description { get; set; }
  public DateTime CreatedAt { get; set; }
  public BookingPartyDto Customer { get; set; } = null!;
  public BookingPartyDto? Worker { get; set; }
  public BookingServiceDto Service { get; set; } = null!;
}

/// <summary>Basic info for a booking party (customer or worker).</summary>
public class BookingPartyDto
{
  public Guid Id { get; set; }
  public string FullName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
}

/// <summary>Service info attached to a booking.</summary>
public class BookingServiceDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
}
