using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IPromotionRepository
{
    Task<Promotion?> FindByIdAsync(Guid id);
    Task<Promotion?> FindByCodeAsync(string code);
    Task<List<Promotion>> GetAllAsync();
    Task<List<Promotion>> GetActivePromotionsAsync();
    Task<Promotion> CreateAsync(Promotion promotion);
    Task UpdateAsync(Promotion promotion);
}
