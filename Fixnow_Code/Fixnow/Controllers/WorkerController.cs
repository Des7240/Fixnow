using System.Security.Claims;
using Fixnow.DTOs.Worker;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// Worker-specific operations: update GPS location, view nearby workers.
/// </summary>
[ApiController]
[Route("api/v1/workers")]
[Authorize]
public class WorkerController : ControllerBase
{
  private readonly IWorkerLocationRepository _locationRepo;
  private readonly IWorkerProfileService _profileService;

  public WorkerController(IWorkerLocationRepository locationRepo, IWorkerProfileService profileService)
  {
    _locationRepo = locationRepo;
    _profileService = profileService;
  }

  /// <summary>
  /// Get public profile of a worker.
  /// </summary>
  [HttpGet("{id:guid}/profile")]
  [ProducesResponseType(typeof(Fixnow.DTOs.WorkerProfile.WorkerProfileDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetWorkerProfile([FromRoute] Guid id)
  {
    var profile = await _profileService.GetProfileAsync(id);
    return Ok(profile);
  }

  /// <summary>
  /// Worker updates their current GPS location.
  /// Must be called periodically so matching engine can find them.
  /// </summary>
  [HttpPut("location")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequestDto request)
  {
    EnsureRole(UserRole.WORKER);
    var workerId = GetCurrentUserId();

    await _locationRepo.UpsertAsync(workerId, request.Lat, request.Lng);
    return Ok(new { message = "Location updated successfully.", lat = request.Lat, lng = request.Lng });
  }

  /// <summary>
  /// Find available workers near a coordinate (CUSTOMER use).
  /// Useful for displaying nearby workers on the map before booking.
  /// </summary>
  [HttpGet("nearby")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetNearbyWorkers(
    [FromQuery] double lat,
    [FromQuery] double lng,
    [FromQuery] Guid serviceId,
    [FromQuery] double radiusKm = 5)
  {
    var workers = await _locationRepo.FindNearbyAvailableWorkersAsync(
      lat, lng, serviceId, radiusMeters: radiusKm * 1000);

    var result = workers.Select(w => new
    {
      workerId = w.WorkerId,
      workerName = w.WorkerName,
      distanceKm = Math.Round(w.DistanceMeters / 1000, 2),
    });

    return Ok(result);
  }

  // ── Helpers ─────────────────────────────────────────────────────────────────

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue("sub")
      ?? throw new UnauthorizedAccessException("User ID not found in token.");
    return Guid.Parse(sub);
  }

  private UserRole GetCurrentUserRole()
  {
    var roleStr = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    return Enum.TryParse<UserRole>(roleStr, out var role) ? role : UserRole.CUSTOMER;
  }

  private void EnsureRole(UserRole required)
  {
    if (GetCurrentUserRole() != required)
      throw new UnauthorizedAccessException($"Only {required} can perform this action.");
  }
}
