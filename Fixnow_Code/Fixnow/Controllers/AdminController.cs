using System.Security.Claims;
using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Kyc;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
  private readonly IAdminService _adminService;

  public AdminController(IAdminService adminService)
  {
    _adminService = adminService;
  }

  [HttpPatch("kyc/{id:guid}")]
  [ProducesResponseType(typeof(KycResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> ReviewKyc([FromRoute] Guid id, [FromBody] ReviewKycDto request)
  {
    var result = await _adminService.ReviewKycAsync(id, GetCurrentUserId(), request);
    return Ok(result);
  }

  [HttpPatch("workers/{id:guid}/suspend")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> SuspendWorker([FromRoute] Guid id)
  {
    await _adminService.SuspendWorkerAsync(id, GetCurrentUserId());
    return NoContent();
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
