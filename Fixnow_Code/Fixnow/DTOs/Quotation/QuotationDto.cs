using Fixnow.Enums;

namespace Fixnow.DTOs.Quotation;

public class CreateQuotationRequestDto
{
  public Guid BookingId { get; set; }
  public List<CreateQuotationItemDto> Items { get; set; } = new();
  public string? Note { get; set; }
}

public class CreateQuotationItemDto
{
  public string ItemName { get; set; } = string.Empty;
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
}

public class QuotationDto
{
  public Guid Id { get; set; }
  public Guid BookingId { get; set; }
  public Guid WorkerId { get; set; }
  public Guid CustomerId { get; set; }
  public decimal Subtotal { get; set; }
  public decimal TotalAmount { get; set; }
  public string? Note { get; set; }
  public QuotationStatus Status { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime ExpiresAt { get; set; }
  
  public List<QuotationItemDto> Items { get; set; } = new();
}

public class QuotationItemDto
{
  public Guid Id { get; set; }
  public string ItemName { get; set; } = string.Empty;
  public int Quantity { get; set; }
  public decimal UnitPrice { get; set; }
  public decimal TotalPrice { get; set; }
}
