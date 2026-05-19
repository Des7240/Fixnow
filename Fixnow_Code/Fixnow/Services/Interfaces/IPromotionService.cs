using Fixnow.DTOs.Promotion;
using Fixnow.Entities;

namespace Fixnow.Services.Interfaces;

public interface IPromotionService
{
    Task<List<PromotionDto>> GetAllPromotionsAsync();
    Task<List<PromotionDto>> GetActivePromotionsAsync();
    Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto request);
    Task<PromotionDto> UpdatePromotionStatusAsync(Guid id, bool isActive);
    Task<ValidatePromotionResultDto> ValidatePromotionAsync(ValidatePromotionDto request);
    Task ApplyPromotionUsageAsync(Guid promotionId, Guid userId);
}
