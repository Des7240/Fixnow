using Fixnow.Data;
using Fixnow.DTOs.Promotion;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepo;
    private readonly AppDbContext _db;

    public PromotionService(IPromotionRepository promotionRepo, AppDbContext db)
    {
        _promotionRepo = promotionRepo;
        _db = db;
    }

    public async Task<List<PromotionDto>> GetAllPromotionsAsync()
    {
        var promos = await _promotionRepo.GetAllAsync();
        return promos.Select(MapToDto).ToList();
    }

    public async Task<List<PromotionDto>> GetActivePromotionsAsync()
    {
        var promos = await _promotionRepo.GetActivePromotionsAsync();
        return promos.Select(MapToDto).ToList();
    }

    public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto request)
    {
        var existing = await _promotionRepo.FindByCodeAsync(request.Code);
        if (existing != null)
            throw new Exception("Mã khuyến mãi đã tồn tại");

        var promo = new Promotion
        {
            Code = request.Code.ToUpper(),
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MaxDiscountAmount = request.MaxDiscountAmount,
            MinOrderValue = request.MinOrderValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MaxUsageLimit = request.MaxUsageLimit,
            ApplicableServiceId = request.ApplicableServiceId,
            IsActive = true
        };

        var created = await _promotionRepo.CreateAsync(promo);
        return MapToDto(created);
    }

    public async Task<PromotionDto> UpdatePromotionStatusAsync(Guid id, bool isActive)
    {
        var promo = await _promotionRepo.FindByIdAsync(id) ?? throw new KeyNotFoundException("Không tìm thấy mã khuyến mãi");
        promo.IsActive = isActive;
        promo.UpdatedAt = DateTime.UtcNow;
        await _promotionRepo.UpdateAsync(promo);
        return MapToDto(promo);
    }

    public async Task<ValidatePromotionResultDto> ValidatePromotionAsync(ValidatePromotionDto request)
    {
        var promo = await _promotionRepo.FindByCodeAsync(request.Code);
        if (promo == null || !promo.IsActive)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = "Mã khuyến mãi không tồn tại hoặc đã bị khoá." };

        var now = DateTime.UtcNow;
        if (now < promo.StartDate)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = "Mã khuyến mãi chưa tới thời gian áp dụng." };
        if (now > promo.EndDate)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = "Mã khuyến mãi đã hết hạn." };

        if (promo.MaxUsageLimit > 0 && promo.CurrentUsageCount >= promo.MaxUsageLimit)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = "Mã khuyến mãi đã hết lượt sử dụng." };

        if (promo.MinOrderValue.HasValue && request.OrderValue < promo.MinOrderValue.Value)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = $"Đơn hàng chưa đạt giá trị tối thiểu để áp dụng ({promo.MinOrderValue.Value}đ)." };

        if (promo.ApplicableServiceId.HasValue && request.ServiceId.HasValue && promo.ApplicableServiceId.Value != request.ServiceId.Value)
            return new ValidatePromotionResultDto { IsValid = false, ErrorMessage = "Mã khuyến mãi không áp dụng cho loại dịch vụ này." };

        decimal discount = 0;
        if (promo.DiscountType == PromotionDiscountType.PERCENTAGE)
        {
            discount = request.OrderValue * (promo.DiscountValue / 100);
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
                discount = promo.MaxDiscountAmount.Value;
        }
        else
        {
            discount = promo.DiscountValue;
            if (discount > request.OrderValue) discount = request.OrderValue;
        }

        return new ValidatePromotionResultDto
        {
            IsValid = true,
            PromotionId = promo.Id,
            DiscountAmount = discount,
            FinalAmount = request.OrderValue - discount
        };
    }

    public async Task ApplyPromotionUsageAsync(Guid promotionId, Guid userId)
    {
        var promo = await _promotionRepo.FindByIdAsync(promotionId);
        if (promo != null)
        {
            promo.CurrentUsageCount++;
            await _promotionRepo.UpdateAsync(promo);

            _db.UserPromotionUsages.Add(new UserPromotionUsage
            {
                UserId = userId,
                PromotionId = promotionId
            });
            await _db.SaveChangesAsync();
        }
    }

    private static PromotionDto MapToDto(Promotion p)
    {
        return new PromotionDto
        {
            Id = p.Id,
            Code = p.Code,
            Description = p.Description,
            DiscountType = p.DiscountType.ToString(),
            DiscountValue = p.DiscountValue,
            MaxDiscountAmount = p.MaxDiscountAmount,
            MinOrderValue = p.MinOrderValue,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            MaxUsageLimit = p.MaxUsageLimit,
            CurrentUsageCount = p.CurrentUsageCount,
            IsActive = p.IsActive,
            ApplicableServiceId = p.ApplicableServiceId
        };
    }
}
