using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Fixnow.DTOs.Kyc;

/// <summary>
/// FormData payload for KYC submission.
/// </summary>
public class SubmitKycDto
{
  [Required]
  [StringLength(20, MinimumLength = 9)]
  public string CitizenIdNumber { get; set; } = string.Empty;

  [Required]
  public IFormFile FrontImage { get; set; } = null!;

  [Required]
  public IFormFile BackImage { get; set; } = null!;

  [Required]
  public IFormFile SelfieImage { get; set; } = null!;

  public IFormFile? CertificateFile { get; set; }
}
