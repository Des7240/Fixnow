namespace Fixnow.DTOs.OpenJob;

public class SubmitOfferRequest
{
  public decimal EstimatedPrice { get; set; }
  public string Analysis { get; set; } = string.Empty;
  public int EstimatedArrivalMinutes { get; set; }
  public int EstimatedRepairTimeMinutes { get; set; }
  public int? WarrantyDays { get; set; }
  public List<Guid> FileIds { get; set; } = new();
}
