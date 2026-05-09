using Fixnow.DTOs.Admin;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service for writing and reading system audit logs.
/// </summary>
public interface IAuditService
{
  Task LogActionAsync(string action, string entityType, Guid? actorId = null, string? actorRole = null, Guid? entityId = null, string? oldData = null, string? newData = null, string? ipAddress = null);
  Task<List<AuditLogDto>> GetRecentAuditLogsAsync(int limit = 100);
}
