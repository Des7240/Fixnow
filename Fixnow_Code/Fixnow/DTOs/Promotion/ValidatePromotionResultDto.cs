namespace Fixnow.DTOs.Promotion;

public class ValidatePromotionResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? PromotionId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
}
