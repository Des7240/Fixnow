using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.WorkerProfile;

public class UpdateWorkerProfileDto
{
  [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
  [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
  public string PhoneNumber { get; set; } = string.Empty;

  [MaxLength(1000)]
  public string? Bio { get; set; }

  [Range(0, 50)]
  public int ExperienceYears { get; set; }
}
