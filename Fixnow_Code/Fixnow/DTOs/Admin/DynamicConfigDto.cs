using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Admin;

public record CreateServiceRequestDto(
    [Required] string Name,
    string? Description,
    string? IconUrl,
    decimal BasePrice,
    int EstimatedDurationMinutes
);

public record UpdateServiceRequestDto(
    string Name,
    string? Description,
    string? IconUrl,
    decimal BasePrice,
    int EstimatedDurationMinutes,
    bool IsActive
);

public record UpdateConfigDto([Required] string Key, [Required] string Value);
public record UpdateCommissionDto([Required] Guid ServiceId, [Required] decimal Percent);
