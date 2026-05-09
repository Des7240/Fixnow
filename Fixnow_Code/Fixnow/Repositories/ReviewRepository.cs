using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <inheritdoc/>
public class ReviewRepository : IReviewRepository
{
  private readonly AppDbContext _db;

  public ReviewRepository(AppDbContext db)
  {
    _db = db;
  }

  /// <inheritdoc/>
  public async Task<WorkerReview> AddAsync(WorkerReview review)
  {
    _db.WorkerReviews.Add(review);
    await _db.SaveChangesAsync();
    return review;
  }

  /// <inheritdoc/>
  public async Task<bool> ExistsForBookingAsync(Guid bookingId)
  {
    return await _db.WorkerReviews.AnyAsync(r => r.BookingId == bookingId);
  }

  /// <inheritdoc/>
  public async Task<IList<WorkerReview>> GetByWorkerIdAsync(Guid workerId)
  {
    return await _db.WorkerReviews
      .Include(r => r.Customer)
      .Where(r => r.WorkerId == workerId)
      .OrderByDescending(r => r.CreatedAt)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task UpsertRatingSummaryAsync(Guid workerId)
  {
    var avg = await _db.WorkerReviews
      .Where(r => r.WorkerId == workerId)
      .AverageAsync(r => (double)r.Rating);

    var count = await _db.WorkerReviews
      .CountAsync(r => r.WorkerId == workerId);

    var summary = await _db.WorkerRatingSummaries
      .FindAsync(workerId);

    if (summary == null)
    {
      _db.WorkerRatingSummaries.Add(new WorkerRatingSummary
      {
        WorkerId = workerId,
        AverageRating = Math.Round(avg, 2),
        TotalReviews = count,
        UpdatedAt = DateTime.UtcNow
      });
    }
    else
    {
      summary.AverageRating = Math.Round(avg, 2);
      summary.TotalReviews = count;
      summary.UpdatedAt = DateTime.UtcNow;
    }

    await _db.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task<WorkerRatingSummary?> GetRatingSummaryAsync(Guid workerId)
  {
    return await _db.WorkerRatingSummaries.FindAsync(workerId);
  }
}
