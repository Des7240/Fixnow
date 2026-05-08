using Fixnow.Enums;

namespace Fixnow.Entities;

/// <summary>
/// KYC submission history for a worker.
/// </summary>
public class WorkerKyc
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid WorkerId { get; set; }
  public string CitizenIdNumber { get; set; } = string.Empty;
  public string CitizenFrontUrl { get; set; } = string.Empty;
  public string CitizenBackUrl { get; set; } = string.Empty;
  public string SelfieUrl { get; set; } = string.Empty;
  public string? CertificateUrl { get; set; }

  public KycStatus Status { get; set; } = KycStatus.PENDING;
  public string? RejectionReason { get; set; }
  
  public Guid? VerifiedBy { get; set; }
  public DateTime? VerifiedAt { get; set; }
  public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User Worker { get; set; } = null!;
  public User? Admin { get; set; }
}
