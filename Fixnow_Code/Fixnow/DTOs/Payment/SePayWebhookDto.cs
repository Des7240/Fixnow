namespace Fixnow.DTOs.Payment;

public class SePayWebhookDto
{
  public long Id { get; set; }
  public string Gateway { get; set; } = string.Empty;
  public string TransactionDate { get; set; } = string.Empty;
  public string AccountNumber { get; set; } = string.Empty;
  public string SubAccount { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public string TransferType { get; set; } = string.Empty;
  public decimal TransferAmount { get; set; }
  public decimal Accumulated { get; set; }
  public string ReferenceCode { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
}
