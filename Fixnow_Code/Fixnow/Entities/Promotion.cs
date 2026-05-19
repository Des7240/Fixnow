using Fixnow.Enums;

namespace Fixnow.Entities;

public class Promotion
{
  public Guid Id { get; set; } = Guid.NewGuid();
  
  public string Code { get; set; } = string.Empty; // e.g. FIXNOW50
  public string Description { get; set; } = string.Empty;
  
  public PromotionDiscountType DiscountType { get; set; }
  public decimal DiscountValue { get; set; } // Percentage or Fixed amount
  public decimal? MaxDiscountAmount { get; set; } // Max cap for percentage discount
  public decimal? MinOrderValue { get; set; } // Minimum order total to apply
  
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  
  public int MaxUsageLimit { get; set; } = 0; // 0 = unlimited
  public int CurrentUsageCount { get; set; } = 0;
  
  public bool IsActive { get; set; } = true;

  // Optional: Null = applies to all services. 
  // If set, only applies to bookings of this service.
  public Guid? ApplicableServiceId { get; set; }
  public ServiceCategory? ApplicableService { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
