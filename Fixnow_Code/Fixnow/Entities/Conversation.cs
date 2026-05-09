namespace Fixnow.Entities;

public class Conversation
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid BookingId { get; set; }
  public Guid CustomerId { get; set; }
  public Guid WorkerId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Booking Booking { get; set; } = null!;
  public User Customer { get; set; } = null!;
  public User Worker { get; set; } = null!;
  public ICollection<Message> Messages { get; set; } = new List<Message>();
}
