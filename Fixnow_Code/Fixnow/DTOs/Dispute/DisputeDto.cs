using Fixnow.Enums;

namespace Fixnow.DTOs.Dispute;

public class CreateDisputeDto
{
  public Guid BookingId { get; set; }
  public string Reason { get; set; } = string.Empty;
}

public class DisputeDto
{
  public Guid Id { get; set; }
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public Guid WorkerId { get; set; }
  public string WorkerName { get; set; } = string.Empty;
  public string Reason { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
  public List<DisputeEvidenceDto> Evidences { get; set; } = new List<DisputeEvidenceDto>();
  public List<RefundDto> Refunds { get; set; } = new List<RefundDto>();
}

public class DisputeEvidenceDto
{
  public Guid Id { get; set; }
  public string FileUrl { get; set; } = string.Empty;
  public Guid UploadedBy { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class RefundRequestDto
{
  public decimal Amount { get; set; }
  public RefundType RefundType { get; set; }
}

public class RefundDto
{
  public Guid Id { get; set; }
  public decimal Amount { get; set; }
  public string RefundType { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
