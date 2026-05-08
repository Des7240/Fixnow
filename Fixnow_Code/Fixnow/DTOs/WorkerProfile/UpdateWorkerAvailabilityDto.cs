using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.WorkerProfile;

public class UpdateWorkerAvailabilityDto
{
  [Required]
  public WorkerAvailability Status { get; set; }
}
