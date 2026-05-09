namespace Fixnow.Entities;

public class QuotationItem
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid QuotationId { get; set; }
  public string ItemName { get; set; } = string.Empty;
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public decimal TotalPrice { get; set; }

  // Navigation
  public Quotation Quotation { get; set; } = null!;
}
