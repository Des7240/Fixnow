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

  [HttpGet("marketplace")]
  [Authorize(Roles = "WORKER")]
  public async Task<ActionResult<IEnumerable<OpenJobResponse>>> GetMarketplaceJobs(
    [FromQuery] double lat, 
    [FromQuery] double lng, 
    [FromQuery] double radius = 10,
    [FromQuery] string? serviceTypes = null,
    [FromQuery] decimal? minBudget = null,
    [FromQuery] decimal? maxBudget = null,
    [FromQuery] string? urgencyLevel = null,
    [FromQuery] string? sort = "latest")
  {
    var workerId = GetCurrentUserId();
    var serviceIdsList = serviceTypes?.Split(',').Select(Guid.Parse).ToList();
    
    var result = await _openJobService.GetMarketplaceJobsAsync(
      workerId, lat, lng, radius, serviceIdsList, minBudget, maxBudget, urgencyLevel, sort);
    return Ok(result);
  }

  [HttpGet("saved")]
  [Authorize(Roles = "WORKER")]
  public async Task<ActionResult<IEnumerable<OpenJobResponse>>> GetSavedJobs()
  {
    var workerId = GetCurrentUserId();
    var result = await _openJobService.GetSavedJobsAsync(workerId);
    return Ok(result);
  }

  [HttpPost("{id}/save")]
  [Authorize(Roles = "WORKER")]
  public async Task<IActionResult> SaveJob(Guid id)
  {
    var workerId = GetCurrentUserId();
    await _openJobService.SaveJobAsync(workerId, id);
    return NoContent();
  }

  [HttpDelete("{id}/save")]
  [Authorize(Roles = "WORKER")]
  public async Task<IActionResult> UnsaveJob(Guid id)
  {
    var workerId = GetCurrentUserId();
    await _openJobService.UnsaveJobAsync(workerId, id);
    return NoContent();
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

  [HttpPut("{id}")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<ActionResult<OpenJobResponse>> UpdateJob(Guid id, [FromBody] CreateOpenJobRequest request)
  {
    var customerId = GetCurrentUserId();
    var result = await _openJobService.UpdateJobAsync(customerId, id, request);
    return Ok(result);
  }

  [HttpPost("{id}/close")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> CloseJob(Guid id, [FromBody] string? reason)
  {
    var customerId = GetCurrentUserId();
    await _openJobService.CloseJobAsync(customerId, id, reason);
    return NoContent();
  }

  [HttpPost("offers/{offerId}/reject")]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<IActionResult> RejectOffer(Guid offerId)
  {
    var customerId = GetCurrentUserId();
    await _openJobService.RejectOfferAsync(customerId, offerId);
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
