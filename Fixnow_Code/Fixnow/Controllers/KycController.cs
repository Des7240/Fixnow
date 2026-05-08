using System.Security.Claims;
using Fixnow.DTOs.Kyc;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/workers/kyc")]
[Authorize(Roles = "WORKER")]
public class KycController : ControllerBase
{
  private readonly IKycService _kycService;

  public KycController(IKycService kycService)
  {
    _kycService = kycService;
  }

  [HttpPost]
  [ProducesResponseType(typeof(KycResponseDto), StatusCodes.Status201Created)]
  public async Task<IActionResult> SubmitKyc([FromForm] SubmitKycDto request)
  {
    var result = await _kycService.SubmitKycAsync(GetCurrentUserId(), request);
    return StatusCode(StatusCodes.Status201Created, result);
  }

  [HttpGet]
  [ProducesResponseType(typeof(KycResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetKycStatus()
  {
    var result = await _kycService.GetKycStatusAsync(GetCurrentUserId());
    return Ok(result);
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
