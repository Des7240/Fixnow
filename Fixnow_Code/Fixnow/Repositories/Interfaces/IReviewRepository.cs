using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository for managing worker reviews and rating summaries.
/// </summary>
public interface IReviewRepository
{
  /// <summary>Adds a new review.</summary>
  Task<WorkerReview> AddAsync(WorkerReview review);

  /// <summary>Checks if a review already exists for a given booking.</summary>
  Task<bool> ExistsForBookingAsync(Guid bookingId);

  /// <summary>Gets all reviews for a given worker, newest first.</summary>
  Task<IList<WorkerReview>> GetByWorkerIdAsync(Guid workerId);

  /// <summary>Upserts the worker's aggregated rating summary.</summary>
  Task UpsertRatingSummaryAsync(Guid workerId);

  /// <summary>Gets the rating summary for a worker.</summary>
  Task<WorkerRatingSummary?> GetRatingSummaryAsync(Guid workerId);
}
