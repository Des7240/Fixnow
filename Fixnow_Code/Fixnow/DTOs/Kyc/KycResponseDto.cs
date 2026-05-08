using Fixnow.Enums;

namespace Fixnow.DTOs.Kyc;

public class KycResponseDto
{
  public Guid Id { get; set; }
  public string CitizenIdNumber { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public string? RejectionReason { get; set; }
  public DateTime SubmittedAt { get; set; }
  public DateTime? VerifiedAt { get; set; }
}
