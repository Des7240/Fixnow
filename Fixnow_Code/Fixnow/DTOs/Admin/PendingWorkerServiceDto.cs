using Fixnow.Enums;

namespace Fixnow.DTOs.Admin;

public class PendingWorkerServiceDto
{
  public Guid WorkerId { get; set; }
  public string WorkerName { get; set; } = string.Empty;
  public string WorkerEmail { get; set; } = string.Empty;
  public Guid ServiceId { get; set; }
  public string ServiceName { get; set; } = string.Empty;
  public WorkerServiceStatus Status { get; set; }
}
