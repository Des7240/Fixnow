using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.WorkerProfile;

public class UpdateWorkerLocationDto
{
  [Range(-90, 90)]
  public double Latitude { get; set; }

  [Range(-180, 180)]
  public double Longitude { get; set; }
}
