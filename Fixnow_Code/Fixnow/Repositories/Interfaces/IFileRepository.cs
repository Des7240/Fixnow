using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository for uploaded file metadata.
/// </summary>
public interface IFileRepository
{
  Task<UploadedFile> AddAsync(UploadedFile file);
  Task<UploadedFile?> GetByIdAsync(Guid id);
  Task DeleteAsync(Guid id);
}
