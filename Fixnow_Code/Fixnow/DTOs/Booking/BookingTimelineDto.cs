using Fixnow.Enums;

namespace Fixnow.DTOs.Booking;

public class BookingTimelineDto
{
  public Guid Id { get; set; }
  public string? OldStatus { get; set; }
  public string NewStatus { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
