using System.ComponentModel.DataAnnotations;

namespace Fixnow.Entities;

public class SystemConfig
{
    [Key]
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
