using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.Promotion;

public class CreatePromotionDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public PromotionDiscountType DiscountType { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderValue { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public int MaxUsageLimit { get; set; } = 0;

    public Guid? ApplicableServiceId { get; set; }
}
