using System.Security.Claims;
using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Kyc;
using Fixnow.DTOs.OpenJob;
using Fixnow.DTOs.Common;
using Fixnow.DTOs.Booking;
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
  private readonly IOpenJobService _openJobService;

  public AdminController(IAdminService adminService, IAuditService auditService, IOpenJobService openJobService)
  {
    _adminService = adminService;
    _auditService = auditService;
    _openJobService = openJobService;
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

  [HttpGet("users")]
  [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAllUsers()
  {
    var result = await _adminService.GetAllUsersAsync();
    return Ok(result);
  }

  [HttpPatch("users/{userId:guid}/status")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> UpdateUserStatus([FromRoute] Guid userId, [FromBody] UpdateUserStatusDto request)
  {
    await _adminService.UpdateUserStatusAsync(userId, request.Status, GetCurrentUserId());
    return NoContent();
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

  [HttpPatch("workers/{id:guid}/activate")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> ActivateWorker([FromRoute] Guid id)
  {
    await _adminService.ActivateWorkerAsync(id, GetCurrentUserId());
    return NoContent();
  }

  [HttpGet("audit-logs")]
  [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAuditLogs()
  {
    var logs = await _auditService.GetRecentAuditLogsAsync();
    return Ok(logs);
  }

  [HttpGet("workers/services/pending")]
  [ProducesResponseType(typeof(List<PendingWorkerServiceDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetPendingWorkerServices()
  {
    var pending = await _adminService.GetPendingWorkerServicesAsync();
    return Ok(pending);
  }

  [HttpPatch("workers/{workerId:guid}/services/{serviceId:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> ReviewWorkerService([FromRoute] Guid workerId, [FromRoute] Guid serviceId, [FromBody] ReviewWorkerServiceDto request)
  {
    await _adminService.ReviewWorkerServiceAsync(workerId, serviceId, GetCurrentUserId(), request);
    return NoContent();
  }

  [HttpGet("open-jobs")]
  [ProducesResponseType(typeof(List<OpenJobResponse>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAllOpenJobs()
  {
    var result = await _openJobService.GetAllJobsForAdminAsync();
    return Ok(result);
  }

  [HttpPost("open-jobs/{id:guid}/moderate")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> ModerateOpenJob([FromRoute] Guid id, [FromBody] ModerationRequest request)
  {
    await _openJobService.ModerateJobAsync(GetCurrentUserId(), id, request);
    return NoContent();
  }

  [HttpDelete("open-jobs/{id:guid}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> DeleteOpenJob([FromRoute] Guid id)
  {
    await _openJobService.DeleteJobAsync(GetCurrentUserId(), id);
    return NoContent();
  }

  [HttpGet("bookings")]
  [ProducesResponseType(typeof(PagedResponseDto<BookingResponseDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetBookings([FromQuery] GetBookingsQueryDto query)
  {
    var result = await _adminService.GetAllBookingsAsync(query);
    return Ok(result);
  }

  [HttpGet("transactions")]
  [ProducesResponseType(typeof(PagedResponseDto<PaymentAdminDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetTransactions([FromQuery] GetTransactionsQueryDto query)
  {
    var result = await _adminService.GetAllTransactionsAsync(query);
    return Ok(result);
  }

  // Phase 6: Dynamic Config & Service CRUD
  [HttpGet("system-configs")]
  public async Task<IActionResult> GetSystemConfigs()
  {
    var result = await _adminService.GetAllConfigsAsync();
    return Ok(result);
  }

  [HttpPut("system-configs")]
  public async Task<IActionResult> UpdateSystemConfig([FromBody] UpdateConfigDto request)
  {
    await _adminService.UpdateSystemConfigAsync(request.Key, request.Value, GetCurrentUserId());
    return NoContent();
  }

  [HttpGet("service-commissions")]
  public async Task<IActionResult> GetServiceCommissions()
  {
    var result = await _adminService.GetAllCommissionsAsync();
    return Ok(result);
  }

  [HttpPut("service-commissions")]
  public async Task<IActionResult> UpdateServiceCommission([FromBody] UpdateCommissionDto request)
  {
    await _adminService.UpdateServiceCommissionAsync(request.ServiceId, request.Percent, GetCurrentUserId());
    return NoContent();
  }

  [HttpGet("services")]
  public async Task<IActionResult> GetAllServices()
  {
    var result = await _adminService.GetAllServicesAsync();
    return Ok(result);
  }

  [HttpPost("services")]
  public async Task<IActionResult> CreateService([FromBody] CreateServiceRequestDto request)
  {
    var result = await _adminService.CreateServiceAsync(request, GetCurrentUserId());
    return CreatedAtAction(nameof(CreateService), new { id = result.Id }, result);
  }

  [HttpPut("services/{id:guid}")]
  public async Task<IActionResult> UpdateService([FromRoute] Guid id, [FromBody] UpdateServiceRequestDto request)
  {
    var result = await _adminService.UpdateServiceAsync(id, request, GetCurrentUserId());
    return Ok(result);
  }

  [HttpDelete("services/{id:guid}")]
  public async Task<IActionResult> DeleteService([FromRoute] Guid id)
  {
    await _adminService.DeleteServiceAsync(id, GetCurrentUserId());
    return NoContent();
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
