using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <inheritdoc/>
public class AuditLogRepository : IAuditLogRepository
{
  private readonly AppDbContext _db;

  public AuditLogRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task AddAsync(AuditLog log)
  {
    _db.AuditLogs.Add(log);
    await _db.SaveChangesAsync();
  }

  public async Task<List<AuditLog>> GetRecentLogsAsync(int limit = 100)
  {
    return await _db.AuditLogs
      .OrderByDescending(a => a.CreatedAt)
      .Take(limit)
      .ToListAsync();
  }
}
