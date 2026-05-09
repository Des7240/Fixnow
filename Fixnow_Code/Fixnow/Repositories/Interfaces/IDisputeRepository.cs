using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IDisputeRepository
{
  Task<Dispute> CreateAsync(Dispute dispute);
  Task<Dispute?> FindByIdAsync(Guid disputeId);
  Task<Dispute?> FindByIdWithDetailsAsync(Guid disputeId);
  Task<List<Dispute>> FindByUserIdAsync(Guid userId);
  Task<List<Dispute>> GetAllAsync();
  Task UpdateAsync(Dispute dispute);
  Task<DisputeEvidence> AddEvidenceAsync(DisputeEvidence evidence);
}

public interface IRefundRepository
{
  Task<Refund> CreateAsync(Refund refund);
  Task<List<Refund>> FindByDisputeIdAsync(Guid disputeId);
  Task UpdateAsync(Refund refund);
}
