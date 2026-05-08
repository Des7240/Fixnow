using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.Admin;

public class ReviewKycDto
{
  [Required]
  public KycStatus Status { get; set; }

  [MaxLength(1000)]
  public string? Reason { get; set; }
}
