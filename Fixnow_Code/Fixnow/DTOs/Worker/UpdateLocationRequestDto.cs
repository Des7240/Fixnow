using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Worker;

/// <summary>
/// DTO for a worker to update their current GPS location.
/// </summary>
public class UpdateLocationRequestDto
{
  [Required]
  [Range(-90, 90)]
  public double Lat { get; set; }

  [Required]
  [Range(-180, 180)]
  public double Lng { get; set; }
}
