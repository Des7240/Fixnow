using Fixnow.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fixnow.Entities;

public class Transaction
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid PaymentId { get; set; }
  public string? GatewayTransactionId { get; set; }
  
  [Column(TypeName = "jsonb")]
  public string? ProviderResponse { get; set; }
  
  public PaymentStatus Status { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Payment Payment { get; set; } = null!;
}
