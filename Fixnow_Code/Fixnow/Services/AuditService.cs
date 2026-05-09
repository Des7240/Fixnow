using Fixnow.DTOs.Admin;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using System.Text.Json;

namespace Fixnow.Services;

/// <inheritdoc/>
public class AuditService : IAuditService
{
  private readonly IAuditLogRepository _auditRepo;

  public AuditService(IAuditLogRepository auditRepo)
  {
    _auditRepo = auditRepo;
  }

  public async Task LogActionAsync(string action, string entityType, Guid? actorId = null, string? actorRole = null, Guid? entityId = null, string? oldData = null, string? newData = null, string? ipAddress = null)
  {
    var log = new AuditLog
    {
      ActorId = actorId,
      ActorRole = actorRole,
      Action = action,
      EntityType = entityType,
      EntityId = entityId,
      OldData = EnsureValidJson(oldData),
      NewData = EnsureValidJson(newData),
      IpAddress = ipAddress,
      CreatedAt = DateTime.UtcNow
    };

    await _auditRepo.AddAsync(log);
  }

  public async Task<List<AuditLogDto>> GetRecentAuditLogsAsync(int limit = 100)
  {
    var logs = await _auditRepo.GetRecentLogsAsync(limit);
    return logs.Select(l => new AuditLogDto
    {
      Id = l.Id,
      ActorId = l.ActorId,
      ActorRole = l.ActorRole,
      Action = l.Action,
      EntityType = l.EntityType,
      EntityId = l.EntityId,
      OldData = l.OldData,
      NewData = l.NewData,
      IpAddress = l.IpAddress,
      CreatedAt = l.CreatedAt
    }).ToList();
  }

  private static string? EnsureValidJson(string? input)
  {
    if (string.IsNullOrWhiteSpace(input)) return null;

    var trimmed = input.Trim();
    
    // Quick check if it looks like a JSON object or array
    if ((trimmed.StartsWith("{") && trimmed.EndsWith("}")) || 
        (trimmed.StartsWith("[") && trimmed.EndsWith("]")))
    {
      try
      {
        using var doc = JsonDocument.Parse(trimmed);
        return trimmed; // It's valid JSON object/array
      }
      catch (JsonException)
      {
        // Not valid JSON, will be treated as plain string below
      }
    }

    // It's a plain string, serialize it to make it a valid JSON string literal
    return JsonSerializer.Serialize(input);
  }
}
