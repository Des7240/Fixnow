using Fixnow.Enums;

namespace Fixnow.DTOs.Wallet;

public class WalletDto
{
  public Guid Id { get; set; }
  public decimal Balance { get; set; }
  public decimal PendingBalance { get; set; }
  public string Status { get; set; } = string.Empty;
}

public class WalletTransactionDto
{
  public Guid Id { get; set; }
  public string Type { get; set; } = string.Empty;
  public decimal Amount { get; set; }
  public decimal BalanceAfter { get; set; }
  public Guid? ReferenceId { get; set; }
  public string? Description { get; set; }
  public DateTime CreatedAt { get; set; }
}

public class WithdrawRequestDto
{
  public decimal Amount { get; set; }
  public string BankName { get; set; } = string.Empty;
  public string AccountNumber { get; set; } = string.Empty;
  public string AccountName { get; set; } = string.Empty;
}

public class WithdrawalDto
{
  public Guid Id { get; set; }
  public decimal Amount { get; set; }
  public string BankName { get; set; } = string.Empty;
  public string AccountNumber { get; set; } = string.Empty;
  public string AccountName { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }
}
