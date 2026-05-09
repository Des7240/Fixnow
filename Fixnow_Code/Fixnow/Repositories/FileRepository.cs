using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;

namespace Fixnow.Repositories;

/// <inheritdoc/>
public class FileRepository : IFileRepository
{
  private readonly AppDbContext _db;

  public FileRepository(AppDbContext db)
  {
    _db = db;
  }

  /// <inheritdoc/>
  public async Task<UploadedFile> AddAsync(UploadedFile file)
  {
    _db.UploadedFiles.Add(file);
    await _db.SaveChangesAsync();
    return file;
  }

  /// <inheritdoc/>
  public async Task<UploadedFile?> GetByIdAsync(Guid id)
  {
    return await _db.UploadedFiles.FindAsync(id);
  }

  /// <inheritdoc/>
  public async Task DeleteAsync(Guid id)
  {
    var file = await _db.UploadedFiles.FindAsync(id);
    if (file != null)
    {
      _db.UploadedFiles.Remove(file);
      await _db.SaveChangesAsync();
    }
  }
}
