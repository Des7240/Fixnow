using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IWalletRepository
{
  Task<Wallet?> FindByUserIdAsync(Guid userId);
  Task<Wallet> CreateAsync(Wallet wallet);
  Task UpdateAsync(Wallet wallet);
}

public interface IWalletTransactionRepository
{
  Task<WalletTransaction> CreateAsync(WalletTransaction transaction);
  Task<List<WalletTransaction>> FindByWalletIdAsync(Guid walletId);
}

public interface IWithdrawalRepository
{
  Task<Withdrawal> CreateAsync(Withdrawal withdrawal);
  Task<List<Withdrawal>> FindByWalletIdAsync(Guid walletId);
}
