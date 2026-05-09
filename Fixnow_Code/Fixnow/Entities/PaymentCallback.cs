using Fixnow.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fixnow.Entities;

public class PaymentCallback
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public PaymentProvider Provider { get; set; }
  
  [Column(TypeName = "jsonb")]
  public string Payload { get; set; } = string.Empty;
  
  public bool Verified { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
