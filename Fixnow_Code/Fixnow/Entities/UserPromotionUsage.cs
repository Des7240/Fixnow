namespace Fixnow.Entities;

public class UserPromotionUsage
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  public Guid PromotionId { get; set; }
  public DateTime UsedAt { get; set; } = DateTime.UtcNow;

  public User User { get; set; } = null!;
  public Promotion Promotion { get; set; } = null!;
}
