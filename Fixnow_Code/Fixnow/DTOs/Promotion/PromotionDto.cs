using Fixnow.Enums;
namespace Fixnow.DTOs.Promotion;

public class PromotionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxUsageLimit { get; set; }
    public int CurrentUsageCount { get; set; }
    public bool IsActive { get; set; }
    public Guid? ApplicableServiceId { get; set; }
}
