using Fixnow.Entities;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service for uploading files and managing metadata.
/// </summary>
public interface IFileService
{
  Task<UploadedFile> UploadFileAsync(IFormFile file, Guid uploaderId, string bucket = "local");
  Task<UploadedFile?> GetFileMetadataAsync(Guid id);
}
