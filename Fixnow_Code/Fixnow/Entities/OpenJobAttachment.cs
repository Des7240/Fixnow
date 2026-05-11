namespace Fixnow.Entities;

public class OpenJobAttachment
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid OpenJobId { get; set; }
  public Guid FileId { get; set; }

  // Navigation
  public OpenJob OpenJob { get; set; } = null!;
  public UploadedFile File { get; set; } = null!;
}
