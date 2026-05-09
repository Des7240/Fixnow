namespace Fixnow.Entities;

/// <summary>
/// Metadata for files uploaded to MinIO or local storage.
/// </summary>
public class UploadedFile
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string FileName { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public long FileSize { get; set; }
  public string Bucket { get; set; } = string.Empty;
  public string ObjectKey { get; set; } = string.Empty;
  public Guid? UploadedBy { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User? Uploader { get; set; }
}
