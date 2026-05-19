using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Promotion;

public class ValidatePromotionDto
{
    [Required]
    public string Code { get; set; } = string.Empty;

    [Required]
    public decimal OrderValue { get; set; }

    public Guid? ServiceId { get; set; }
}
