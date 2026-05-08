using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Kyc;

namespace Fixnow.Services.Interfaces;

public interface IAdminService
{
  Task<KycResponseDto> ReviewKycAsync(Guid kycId, Guid adminId, ReviewKycDto request);
  Task SuspendWorkerAsync(Guid workerId, Guid adminId);
}
