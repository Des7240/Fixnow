namespace Fixnow.Entities;

public class MessageAttachment
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid MessageId { get; set; }
  public Guid FileId { get; set; }

  // Navigation
  public Message Message { get; set; } = null!;
  public UploadedFile File { get; set; } = null!;
}
