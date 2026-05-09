using System.Security.Claims;
using Fixnow.DTOs.Review;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// API for submitting and viewing worker reviews.
/// </summary>
[ApiController]
[Route("api/v1/reviews")]
public class ReviewController : ControllerBase
{
  private readonly IReviewService _reviewService;

  public ReviewController(IReviewService reviewService)
  {
    _reviewService = reviewService;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  /// <summary>POST /api/v1/reviews — submit a review for a completed booking.</summary>
  [HttpPost]
  [Authorize(Roles = "CUSTOMER")]
  public async Task<ActionResult<ReviewResponseDto>> SubmitReview([FromBody] SubmitReviewDto dto)
  {
    try
    {
      var result = await _reviewService.SubmitReviewAsync(CurrentUserId, dto);
      return CreatedAtAction(nameof(GetWorkerReviews), new { workerId = result.Id }, result);
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>GET /api/v1/reviews/workers/{workerId} — get all reviews for a worker.</summary>
  [HttpGet("workers/{workerId:guid}")]
  public async Task<ActionResult<IList<ReviewResponseDto>>> GetWorkerReviews(Guid workerId)
  {
    var result = await _reviewService.GetWorkerReviewsAsync(workerId);
    return Ok(result);
  }

  /// <summary>GET /api/v1/reviews/workers/{workerId}/summary — get rating summary.</summary>
  [HttpGet("workers/{workerId:guid}/summary")]
  public async Task<ActionResult<WorkerRatingSummaryDto>> GetWorkerRatingSummary(Guid workerId)
  {
    var result = await _reviewService.GetWorkerRatingSummaryAsync(workerId);
    return Ok(result);
  }
}
