using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Kyc;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class AdminService : IAdminService
{
  private readonly IWorkerKycRepository _kycRepo;
  private readonly IUserRepository _userRepo;

  public AdminService(IWorkerKycRepository kycRepo, IUserRepository userRepo)
  {
    _kycRepo = kycRepo;
    _userRepo = userRepo;
  }

  public async Task<KycResponseDto> ReviewKycAsync(Guid kycId, Guid adminId, ReviewKycDto request)
  {
    var kyc = await _kycRepo.FindByIdAsync(kycId)
      ?? throw new KeyNotFoundException("KYC record not found.");

    kyc.Status = request.Status;
    kyc.RejectionReason = request.Reason;
    kyc.VerifiedBy = adminId;
    kyc.VerifiedAt = DateTime.UtcNow;

    await _kycRepo.UpdateAsync(kyc);

    return new KycResponseDto
    {
      Id = kyc.Id,
      CitizenIdNumber = kyc.CitizenIdNumber,
      Status = kyc.Status.ToString(),
      RejectionReason = kyc.RejectionReason,
      SubmittedAt = kyc.SubmittedAt,
      VerifiedAt = kyc.VerifiedAt
    };
  }

  public async Task SuspendWorkerAsync(Guid workerId, Guid adminId)
  {
    var worker = await _userRepo.FindByIdAsync(workerId)
      ?? throw new KeyNotFoundException("Worker not found.");

    worker.Status = "BANNED";
    await _userRepo.UpdateAsync(worker);
  }
}
