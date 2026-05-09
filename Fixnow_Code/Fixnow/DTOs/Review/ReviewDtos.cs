using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Review;

public class SubmitReviewDto
{
  [Required]
  public Guid BookingId { get; set; }

  [Range(1, 5)]
  public int Rating { get; set; }

  [MaxLength(1000)]
  public string? Comment { get; set; }
}

public class ReviewResponseDto
{
  public Guid Id { get; set; }
  public Guid BookingId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public int Rating { get; set; }
  public string? Comment { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class WorkerRatingSummaryDto
{
  public double AverageRating { get; set; }
  public int TotalReviews { get; set; }
}
