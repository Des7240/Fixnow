using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Booking;

/// <summary>
/// DTO for creating a new booking request.
/// </summary>
public class CreateBookingRequestDto
{
  [Required]
  public Guid ServiceId { get; set; }

  [Required]
  [MaxLength(500)]
  public string Address { get; set; } = string.Empty;

  [Required]
  [Range(-90, 90)]
  public double Lat { get; set; }

  [Required]
  [Range(-180, 180)]
  public double Lng { get; set; }

  [MaxLength(1000)]
  public string? Description { get; set; }
}
