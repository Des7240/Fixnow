using Fixnow.Data;
using Fixnow.DTOs.Review;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

/// <inheritdoc/>
public class ReviewService : IReviewService
{
  private readonly IReviewRepository _reviewRepo;
  private readonly AppDbContext _db;

  public ReviewService(IReviewRepository reviewRepo, AppDbContext db)
  {
    _reviewRepo = reviewRepo;
    _db = db;
  }

  /// <inheritdoc/>
  public async Task<ReviewResponseDto> SubmitReviewAsync(Guid customerId, SubmitReviewDto dto)
  {
    // Validate booking belongs to customer and is COMPLETED
    var booking = await _db.Bookings
      .FirstOrDefaultAsync(b => b.Id == dto.BookingId && b.CustomerId == customerId);

    if (booking == null)
      throw new InvalidOperationException("Booking not found or not authorized.");

    if (booking.Status != BookingStatus.COMPLETED)
      throw new InvalidOperationException("Can only review a COMPLETED booking.");

    // Prevent duplicate review
    if (await _reviewRepo.ExistsForBookingAsync(dto.BookingId))
      throw new InvalidOperationException("A review already exists for this booking.");

    var review = new WorkerReview
    {
      BookingId = dto.BookingId,
      CustomerId = customerId,
      WorkerId = booking.WorkerId!.Value,
      Rating = dto.Rating,
      Comment = dto.Comment,
      CreatedAt = DateTime.UtcNow
    };

    var saved = await _reviewRepo.AddAsync(review);

    // Update worker's aggregated rating
    await _reviewRepo.UpsertRatingSummaryAsync(booking.WorkerId!.Value);

    // Fetch customer name for response
    var customer = await _db.Users.FindAsync(customerId);

    return new ReviewResponseDto
    {
      Id = saved.Id,
      BookingId = saved.BookingId,
      CustomerName = customer?.FullName ?? "Unknown",
      CustomerAvatar = customer?.AvatarUrl,
      Rating = saved.Rating,
      Comment = saved.Comment,
      CreatedAt = saved.CreatedAt
    };
  }

  /// <inheritdoc/>
  public async Task<IList<ReviewResponseDto>> GetWorkerReviewsAsync(Guid workerId)
  {
    var reviews = await _reviewRepo.GetByWorkerIdAsync(workerId);
    return reviews.Select(r => new ReviewResponseDto
    {
      Id = r.Id,
      BookingId = r.BookingId,
      CustomerName = r.Customer?.FullName ?? "Unknown",
      CustomerAvatar = r.Customer?.AvatarUrl,
      Rating = r.Rating,
      Comment = r.Comment,
      CreatedAt = r.CreatedAt
    }).ToList();
  }

  /// <inheritdoc/>
  public async Task<WorkerRatingSummaryDto> GetWorkerRatingSummaryAsync(Guid workerId)
  {
    var summary = await _reviewRepo.GetRatingSummaryAsync(workerId);
    return new WorkerRatingSummaryDto
    {
      AverageRating = summary?.AverageRating ?? 0,
      TotalReviews = summary?.TotalReviews ?? 0
    };
  }
}
