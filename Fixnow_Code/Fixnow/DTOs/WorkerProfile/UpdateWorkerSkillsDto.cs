using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.WorkerProfile;

public class UpdateWorkerSkillsDto
{
  [Required]
  public List<Guid> ServiceIds { get; set; } = new();
}
