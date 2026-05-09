using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <inheritdoc/>
public class FileService : IFileService
{
  private readonly IWebHostEnvironment _env;
  private readonly IFileRepository _fileRepo;

  public FileService(IWebHostEnvironment env, IFileRepository fileRepo)
  {
    _env = env;
    _fileRepo = fileRepo;
  }

  /// <inheritdoc/>
  public async Task<UploadedFile> UploadFileAsync(IFormFile file, Guid uploaderId, string bucket = "local")
  {
    if (file == null || file.Length == 0)
      throw new ArgumentException("File is empty.");

    // Validate size (e.g. max 5MB)
    if (file.Length > 5 * 1024 * 1024)
      throw new ArgumentException("File size exceeds 5MB.");

    var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
    if (!allowedTypes.Contains(file.ContentType))
      throw new ArgumentException("Invalid file type.");

    // Create uploads directory if not exists
    var uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", bucket);
    if (!Directory.Exists(uploadPath))
    {
      Directory.CreateDirectory(uploadPath);
    }

    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
    var filePath = Path.Combine(uploadPath, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
      await file.CopyToAsync(stream);
    }

    var uploadedFile = new UploadedFile
    {
      FileName = file.FileName,
      ContentType = file.ContentType,
      FileSize = file.Length,
      Bucket = bucket,
      ObjectKey = $"/uploads/{bucket}/{fileName}",
      UploadedBy = uploaderId
    };

    return await _fileRepo.AddAsync(uploadedFile);
  }

  /// <inheritdoc/>
  public Task<UploadedFile?> GetFileMetadataAsync(Guid id)
  {
    return _fileRepo.GetByIdAsync(id);
  }
}
