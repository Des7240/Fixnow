using Fixnow.Enums;

namespace Fixnow.DTOs.OpenJob;

public class ModerationRequest
{
    public ModerationStatus Status { get; set; }
    public string? Reason { get; set; }
}
