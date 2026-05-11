namespace Fixnow.DTOs.OpenJob;

public class CreateOpenJobRequest
{
  public Guid ServiceId { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }
  public int RadiusKm { get; set; }
  public List<Guid> FileIds { get; set; } = new();
}
