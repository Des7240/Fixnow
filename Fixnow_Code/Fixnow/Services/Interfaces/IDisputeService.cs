using Fixnow.DTOs.Dispute;

namespace Fixnow.Services.Interfaces;

public interface IDisputeService
{
  Task<DisputeDto> CreateDisputeAsync(Guid customerId, CreateDisputeDto request);
  Task<DisputeEvidenceDto> AddEvidenceAsync(Guid disputeId, Guid uploaderId, IFormFile file);
  Task<DisputeDto> GetDisputeAsync(Guid disputeId);
  Task<List<DisputeDto>> GetMyDisputesAsync(Guid userId);
  
  // Admin methods
  Task<List<DisputeDto>> GetAllDisputesAsync();
  Task<DisputeDto> ProcessRefundAsync(Guid adminId, Guid disputeId, RefundRequestDto request);
  Task<DisputeDto> CloseDisputeAsync(Guid adminId, Guid disputeId);
}
