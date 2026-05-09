using Fixnow.Enums;

namespace Fixnow.Entities;

public class Withdrawal
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid WalletId { get; set; }
  
  public decimal Amount { get; set; }
  
  public string BankName { get; set; } = string.Empty;
  public string AccountNumber { get; set; } = string.Empty;
  public string AccountName { get; set; } = string.Empty;
  
  public WithdrawalStatus Status { get; set; } = WithdrawalStatus.PENDING;
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Wallet Wallet { get; set; } = null!;
}
