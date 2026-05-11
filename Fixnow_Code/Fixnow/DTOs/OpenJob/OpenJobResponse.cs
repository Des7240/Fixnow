using Fixnow.Enums;

namespace Fixnow.DTOs.OpenJob;

public class OpenJobResponse
{
  public Guid Id { get; set; }
  public Guid CustomerId { get; set; }
  public string CustomerName { get; set; } = string.Empty;
  public Guid ServiceId { get; set; }
  public string ServiceName { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Address { get; set; } = string.Empty;
  public double Lat { get; set; }
  public double Lng { get; set; }
  public int RadiusKm { get; set; }
  public OpenJobStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public List<string> FileUrls { get; set; } = new();
  public int OfferCount { get; set; }
}
