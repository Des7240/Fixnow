using Fixnow.Enums;

namespace Fixnow.Entities;

public class WalletTransaction
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid WalletId { get; set; }
  
  public TransactionType Type { get; set; }
  
  public decimal Amount { get; set; }
  public decimal BalanceBefore { get; set; }
  public decimal BalanceAfter { get; set; }
  
  public Guid? ReferenceId { get; set; }
  public string? Description { get; set; }
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Wallet Wallet { get; set; } = null!;
}
