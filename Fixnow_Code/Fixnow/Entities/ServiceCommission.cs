using System.ComponentModel.DataAnnotations;

namespace Fixnow.Entities;

public class ServiceCommission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ServiceId { get; set; }
    public ServiceCategory? Service { get; set; }
    
    public decimal CommissionPercent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
