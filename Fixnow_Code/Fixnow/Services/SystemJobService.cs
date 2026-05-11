using Fixnow.Data;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

public class SystemJobService : ISystemJobService
{
  private readonly AppDbContext _db;
  private readonly IOpenJobService _openJobService;
  private readonly ILogger<SystemJobService> _logger;

  public SystemJobService(AppDbContext db, IOpenJobService openJobService, ILogger<SystemJobService> logger)
  {
    _db = db;
    _openJobService = openJobService;
    _logger = logger;
  }

  public async Task CleanupExpiredDataAsync()
  {
    _logger.LogInformation("Running system cleanup job...");

    await _openJobService.ProcessExpiredJobsAsync();
    
    var thresholdDate = DateTime.UtcNow.AddDays(-30);

    // Delete old notifications
    var oldNotifications = await _db.Notifications
      .Where(n => n.CreatedAt < thresholdDate)
      .ToListAsync();

    if (oldNotifications.Any())
    {
      _db.Notifications.RemoveRange(oldNotifications);
      var count = await _db.SaveChangesAsync();
      _logger.LogInformation("Deleted {Count} old notifications.", count);
    }

    // Delete old audit logs
    var oldAuditLogs = await _db.AuditLogs
      .Where(a => a.CreatedAt < thresholdDate)
      .ToListAsync();

    if (oldAuditLogs.Any())
    {
      _db.AuditLogs.RemoveRange(oldAuditLogs);
      var count = await _db.SaveChangesAsync();
      _logger.LogInformation("Deleted {Count} old audit logs.", count);
    }

    _logger.LogInformation("System cleanup job completed.");
  }
}
