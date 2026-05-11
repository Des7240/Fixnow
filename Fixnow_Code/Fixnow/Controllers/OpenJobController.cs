using Fixnow.DTOs.OpenJob;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/open-jobs")]
[Authorize]
public class OpenJobController : ControllerBase
{
  private readonly IOpenJobService _openJobService;

  public OpenJobController(IOpenJobService openJobService)
  {
    _openJobService = openJobService;
  }

  [HttpPost]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<ActionResult<OpenJobResponse>> CreateJob([FromBody] CreateOpenJobRequest request)
  {
    var customerId = GetCurrentUserId();
    var result = await _openJobService.CreateJobAsync(customerId, request);
    return Ok(result);
  }

  [HttpGet("my-jobs")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<ActionResult<IEnumerable<OpenJobResponse>>> GetMyJobs()
  {
    var customerId = GetCurrentUserId();
    var result = await _openJobService.GetCustomerJobsAsync(customerId);
    return Ok(result);
  }

  [HttpGet("nearby")]
  [Authorize(Roles = "WORKER")]
  public async Task<ActionResult<IEnumerable<OpenJobResponse>>> GetNearbyJobs([FromQuery] double lat, [FromQuery] double lng)
  {
    var workerId = GetCurrentUserId();
    var result = await _openJobService.GetNearbyJobsAsync(workerId, lat, lng);
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<OpenJobResponse>> GetJobDetails(Guid id)
  {
    var result = await _openJobService.GetJobDetailsAsync(id);
    return Ok(result);
  }

  [HttpPost("{id}/offers")]
  [Authorize(Roles = "WORKER")]
  public async Task<ActionResult<OfferResponse>> SubmitOffer(Guid id, [FromBody] SubmitOfferRequest request)
  {
    var workerId = GetCurrentUserId();
    var result = await _openJobService.SubmitOfferAsync(workerId, id, request);
    return Ok(result);
  }

  [HttpGet("{id}/offers")]
  public async Task<ActionResult<IEnumerable<OfferResponse>>> GetJobOffers(Guid id)
  {
    var result = await _openJobService.GetJobOffersAsync(id);
    return Ok(result);
  }

  [HttpPost("{id}/select-worker")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> SelectWorker(Guid id, [FromBody] SelectWorkerRequest request)
  {
    var customerId = GetCurrentUserId();
    await _openJobService.SelectWorkerAsync(customerId, id, request.OfferId);
    return NoContent();
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? throw new UnauthorizedAccessException("User ID not found in token.");
    return Guid.Parse(sub);
  }
}
