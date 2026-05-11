using System.ComponentModel.DataAnnotations;

namespace Fixnow.Entities;

public class SavedOpenJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkerId { get; set; }
    public Guid OpenJobId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Worker { get; set; } = null!;
    public OpenJob OpenJob { get; set; } = null!;
}
