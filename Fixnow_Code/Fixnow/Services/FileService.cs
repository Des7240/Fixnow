using Amazon.S3;
using Amazon.S3.Transfer;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

/// <inheritdoc/>
public class FileService : IFileService
{
  private readonly IWebHostEnvironment _env;
  private readonly IFileRepository _fileRepo;
  private readonly IAmazonS3? _s3Client;
  private readonly IConfiguration _config;

  public FileService(IWebHostEnvironment env, IFileRepository fileRepo, IConfiguration config, IServiceProvider serviceProvider)
  {
    _env = env;
    _fileRepo = fileRepo;
    _config = config;
    _s3Client = serviceProvider.GetService<IAmazonS3>();
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

    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
    string objectKey;

    var r2BucketName = _config["CloudflareR2:BucketName"];
    var r2PublicUrl = _config["CloudflareR2:PublicUrl"];

    // Use Cloudflare R2 if configured and client is available
    if (_s3Client != null && !string.IsNullOrEmpty(r2BucketName))
    {
      using var newMemoryStream = new MemoryStream();
      await file.CopyToAsync(newMemoryStream);
      newMemoryStream.Position = 0;

      var uploadRequest = new TransferUtilityUploadRequest
      {
          InputStream = newMemoryStream,
          Key = fileName,
          BucketName = r2BucketName,
          ContentType = file.ContentType,
          DisablePayloadSigning = true // Recommended for R2 to improve performance
      };

      var fileTransferUtility = new TransferUtility(_s3Client);
      await fileTransferUtility.UploadAsync(uploadRequest);

      objectKey = !string.IsNullOrEmpty(r2PublicUrl) ? $"{r2PublicUrl.TrimEnd('/')}/{fileName}" : fileName;
      bucket = "r2"; // update bucket name to r2
    }
    else
    {
      // Fallback to local storage
      var uploadPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", bucket);
      if (!Directory.Exists(uploadPath))
      {
        Directory.CreateDirectory(uploadPath);
      }

      var filePath = Path.Combine(uploadPath, fileName);
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        await file.CopyToAsync(stream);
      }
      objectKey = $"/uploads/{bucket}/{fileName}";
    }

    var uploadedFile = new UploadedFile
    {
      FileName = file.FileName,
      ContentType = file.ContentType,
      FileSize = file.Length,
      Bucket = bucket,
      ObjectKey = objectKey,
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
