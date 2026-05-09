namespace Fixnow.Entities;

/// <summary>
/// In-app notification record for a user.
/// </summary>
public class Notification
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty; // e.g. BOOKING_ACCEPTED, BOOKING_COMPLETED
  public bool IsRead { get; set; } = false;
  public Guid? ReferenceId { get; set; } // e.g. BookingId
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User User { get; set; } = null!;
}
