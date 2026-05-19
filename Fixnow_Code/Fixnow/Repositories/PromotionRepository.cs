using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly AppDbContext _context;

    public PromotionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Promotion?> FindByIdAsync(Guid id)
    {
        return await _context.Promotions.FindAsync(id);
    }

    public async Task<Promotion?> FindByCodeAsync(string code)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code.ToUpper() == code.ToUpper());
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        return await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetActivePromotionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Promotions
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && (p.MaxUsageLimit == 0 || p.CurrentUsageCount < p.MaxUsageLimit))
            .OrderBy(p => p.EndDate)
            .ToListAsync();
    }

    public async Task<Promotion> CreateAsync(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
        return promotion;
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }
}
