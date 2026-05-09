using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository for Audit logs.
/// </summary>
public interface IAuditLogRepository
{
  Task AddAsync(AuditLog log);
  Task<List<AuditLog>> GetRecentLogsAsync(int limit = 100);
}
