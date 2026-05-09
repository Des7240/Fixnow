using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class WalletRepository : IWalletRepository
{
  private readonly AppDbContext _db;
  public WalletRepository(AppDbContext db) { _db = db; }

  public async Task<Wallet?> FindByUserIdAsync(Guid userId)
  {
    return await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
  }

  public async Task<Wallet> CreateAsync(Wallet wallet)
  {
    _db.Wallets.Add(wallet);
    await _db.SaveChangesAsync();
    return wallet;
  }

  public async Task UpdateAsync(Wallet wallet)
  {
    _db.Wallets.Update(wallet);
    await _db.SaveChangesAsync();
  }
}

public class WalletTransactionRepository : IWalletTransactionRepository
{
  private readonly AppDbContext _db;
  public WalletTransactionRepository(AppDbContext db) { _db = db; }

  public async Task<WalletTransaction> CreateAsync(WalletTransaction transaction)
  {
    _db.WalletTransactions.Add(transaction);
    await _db.SaveChangesAsync();
    return transaction;
  }

  public async Task<List<WalletTransaction>> FindByWalletIdAsync(Guid walletId)
  {
    return await _db.WalletTransactions
      .Where(t => t.WalletId == walletId)
      .OrderByDescending(t => t.CreatedAt)
      .ToListAsync();
  }
}

public class WithdrawalRepository : IWithdrawalRepository
{
  private readonly AppDbContext _db;
  public WithdrawalRepository(AppDbContext db) { _db = db; }

  public async Task<Withdrawal> CreateAsync(Withdrawal withdrawal)
  {
    _db.Withdrawals.Add(withdrawal);
    await _db.SaveChangesAsync();
    return withdrawal;
  }

  public async Task<List<Withdrawal>> FindByWalletIdAsync(Guid walletId)
  {
    return await _db.Withdrawals
      .Where(w => w.WalletId == walletId)
      .OrderByDescending(w => w.CreatedAt)
      .ToListAsync();
  }
}
