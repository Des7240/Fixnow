namespace Fixnow.DTOs.File;

public class FileResponseDto
{
  public Guid Id { get; set; }
  public string FileName { get; set; } = string.Empty;
  public string ObjectKey { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long FileSize { get; set; }
  public DateTime CreatedAt { get; set; }
}
