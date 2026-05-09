using Fixnow.DTOs.Review;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service for submitting and querying worker reviews.
/// </summary>
public interface IReviewService
{
  /// <summary>Submits a review for a completed booking.</summary>
  Task<ReviewResponseDto> SubmitReviewAsync(Guid customerId, SubmitReviewDto dto);

  /// <summary>Gets all reviews for a given worker.</summary>
  Task<IList<ReviewResponseDto>> GetWorkerReviewsAsync(Guid workerId);

  /// <summary>Gets aggregated rating summary for a worker.</summary>
  Task<WorkerRatingSummaryDto> GetWorkerRatingSummaryAsync(Guid workerId);
}
