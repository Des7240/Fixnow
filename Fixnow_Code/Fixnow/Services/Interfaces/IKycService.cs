using Fixnow.DTOs.Kyc;

namespace Fixnow.Services.Interfaces;

public interface IKycService
{
  Task<KycResponseDto> SubmitKycAsync(Guid workerId, SubmitKycDto request);
  Task<KycResponseDto> GetKycStatusAsync(Guid workerId);
}
