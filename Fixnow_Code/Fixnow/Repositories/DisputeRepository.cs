using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class DisputeRepository : IDisputeRepository
{
  private readonly AppDbContext _db;
  public DisputeRepository(AppDbContext db) { _db = db; }

  public async Task<Dispute> CreateAsync(Dispute dispute)
  {
    _db.Disputes.Add(dispute);
    await _db.SaveChangesAsync();
    return dispute;
  }

  public async Task<Dispute?> FindByIdAsync(Guid disputeId)
  {
    return await _db.Disputes.FindAsync(disputeId);
  }

  public async Task<Dispute?> FindByIdWithDetailsAsync(Guid disputeId)
  {
    return await _db.Disputes
      .Include(d => d.Evidences)
      .Include(d => d.Refunds)
      .FirstOrDefaultAsync(d => d.Id == disputeId);
  }

  public async Task<List<Dispute>> FindByUserIdAsync(Guid userId)
  {
    return await _db.Disputes
      .Where(d => d.CustomerId == userId || d.WorkerId == userId)
      .OrderByDescending(d => d.CreatedAt)
      .ToListAsync();
  }

  public async Task<List<Dispute>> GetAllAsync()
  {
    return await _db.Disputes
      .OrderByDescending(d => d.CreatedAt)
      .ToListAsync();
  }

  public async Task UpdateAsync(Dispute dispute)
  {
    _db.Disputes.Update(dispute);
    await _db.SaveChangesAsync();
  }

  public async Task<DisputeEvidence> AddEvidenceAsync(DisputeEvidence evidence)
  {
    _db.DisputeEvidences.Add(evidence);
    await _db.SaveChangesAsync();
    return evidence;
  }
}

public class RefundRepository : IRefundRepository
{
  private readonly AppDbContext _db;
  public RefundRepository(AppDbContext db) { _db = db; }

  public async Task<Refund> CreateAsync(Refund refund)
  {
    _db.Refunds.Add(refund);
    await _db.SaveChangesAsync();
    return refund;
  }

  public async Task<List<Refund>> FindByDisputeIdAsync(Guid disputeId)
  {
    return await _db.Refunds
      .Where(r => r.DisputeId == disputeId)
      .ToListAsync();
  }

  public async Task UpdateAsync(Refund refund)
  {
    _db.Refunds.Update(refund);
    await _db.SaveChangesAsync();
  }
}
