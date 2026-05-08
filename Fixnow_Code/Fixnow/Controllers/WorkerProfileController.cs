using System.Security.Claims;
using Fixnow.DTOs.WorkerProfile;
using Fixnow.Enums;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/workers/profile")]
[Authorize(Roles = "WORKER")]
public class WorkerProfileController : ControllerBase
{
  private readonly IWorkerProfileService _profileService;

  public WorkerProfileController(IWorkerProfileService profileService)
  {
    _profileService = profileService;
  }

  [HttpGet]
  [ProducesResponseType(typeof(WorkerProfileDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetProfile()
  {
    var profile = await _profileService.GetProfileAsync(GetCurrentUserId());
    return Ok(profile);
  }

  [HttpPost]
  [ProducesResponseType(typeof(WorkerProfileDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateWorkerProfileDto request)
  {
    var profile = await _profileService.UpdateProfileAsync(GetCurrentUserId(), request);
    return Ok(profile);
  }

  [HttpPatch("availability")]
  [ProducesResponseType(typeof(WorkerProfileDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateAvailability([FromBody] UpdateWorkerAvailabilityDto request)
  {
    var profile = await _profileService.UpdateAvailabilityAsync(GetCurrentUserId(), request.Status);
    return Ok(profile);
  }

  [HttpPost("skills")]
  [ProducesResponseType(typeof(WorkerProfileDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateSkills([FromBody] UpdateWorkerSkillsDto request)
  {
    var profile = await _profileService.UpdateSkillsAsync(GetCurrentUserId(), request.ServiceIds);
    return Ok(profile);
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
