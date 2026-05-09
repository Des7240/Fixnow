namespace Fixnow.Entities;

public class DisputeEvidence
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid DisputeId { get; set; }
  
  public string FileUrl { get; set; } = string.Empty;
  public Guid UploadedBy { get; set; }
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Dispute Dispute { get; set; } = null!;
  public User Uploader { get; set; } = null!;
}
