namespace Fixnow.Entities;

public class Wallet
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid UserId { get; set; }
  
  public decimal Balance { get; set; } = 0;
  public decimal PendingBalance { get; set; } = 0;
  
  public string Status { get; set; } = "ACTIVE";
  
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public User User { get; set; } = null!;
  public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
  public ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
}
