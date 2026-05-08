using Fixnow.DTOs.Kyc;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Fixnow.Services;

public class KycService : IKycService
{
  private readonly IWorkerKycRepository _kycRepo;
  private readonly IWebHostEnvironment _env;

  public KycService(IWorkerKycRepository kycRepo, IWebHostEnvironment env)
  {
    _kycRepo = kycRepo;
    _env = env;
  }

  public async Task<KycResponseDto> SubmitKycAsync(Guid workerId, SubmitKycDto request)
  {
    // Upload files to wwwroot/uploads/kyc
    var frontUrl = await UploadFileAsync(request.FrontImage, "kyc", workerId);
    var backUrl = await UploadFileAsync(request.BackImage, "kyc", workerId);
    var selfieUrl = await UploadFileAsync(request.SelfieImage, "kyc", workerId);
    
    string? certUrl = null;
    if (request.CertificateFile != null)
    {
      certUrl = await UploadFileAsync(request.CertificateFile, "certs", workerId);
    }

    var kyc = new WorkerKyc
    {
      WorkerId = workerId,
      CitizenIdNumber = request.CitizenIdNumber,
      CitizenFrontUrl = frontUrl,
      CitizenBackUrl = backUrl,
      SelfieUrl = selfieUrl,
      CertificateUrl = certUrl,
      Status = KycStatus.PENDING
    };

    var created = await _kycRepo.CreateAsync(kyc);
    return MapToDto(created);
  }

  public async Task<KycResponseDto> GetKycStatusAsync(Guid workerId)
  {
    var kyc = await _kycRepo.FindLatestByWorkerIdAsync(workerId)
      ?? throw new KeyNotFoundException("No KYC submission found for this worker.");

    return MapToDto(kyc);
  }

  private async Task<string> UploadFileAsync(IFormFile file, string folder, Guid workerId)
  {
    if (file == null || file.Length == 0)
      throw new ArgumentException("Invalid file.");

    var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", folder, workerId.ToString());
    Directory.CreateDirectory(uploadsFolder);

    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
    var filePath = Path.Combine(uploadsFolder, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
      await file.CopyToAsync(stream);
    }

    // Return relative URL
    return $"/uploads/{folder}/{workerId}/{fileName}";
  }

  private static KycResponseDto MapToDto(WorkerKyc kyc)
  {
    return new KycResponseDto
    {
      Id = kyc.Id,
      CitizenIdNumber = kyc.CitizenIdNumber,
      Status = kyc.Status.ToString(),
      RejectionReason = kyc.RejectionReason,
      SubmittedAt = kyc.SubmittedAt,
      VerifiedAt = kyc.VerifiedAt
    };
  }
}
