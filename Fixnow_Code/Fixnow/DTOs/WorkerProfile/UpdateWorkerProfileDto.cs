using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.WorkerProfile;

public class UpdateWorkerProfileDto
{
  [MaxLength(1000)]
  public string? Bio { get; set; }

  [Range(0, 50)]
  public int ExperienceYears { get; set; }
}
