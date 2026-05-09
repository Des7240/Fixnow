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
  private readonly IAuditService _auditService;

  public AdminController(IAdminService adminService, IAuditService auditService)
  {
    _adminService = adminService;
    _auditService = auditService;
  }

  [HttpGet("dashboard")]
  [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDashboardSummary()
  {
    var result = await _adminService.GetDashboardSummaryAsync();
    return Ok(result);
  }

  [HttpGet("kyc")]
  [ProducesResponseType(typeof(List<KycResponseDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAllKycs()
  {
    var result = await _adminService.GetAllKycsAsync();
    return Ok(result);
  }

  [HttpGet("workers")]
  [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAllWorkers()
  {
    var result = await _adminService.GetAllWorkersAsync();
    return Ok(result);
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

  [HttpGet("audit-logs")]
  [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAuditLogs()
  {
    var logs = await _auditService.GetRecentAuditLogsAsync();
    return Ok(logs);
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
