using Fixnow.Data;
using Fixnow.DTOs.Wallet;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class WalletService : IWalletService
{
  private readonly IWalletRepository _walletRepo;
  private readonly IWalletTransactionRepository _transactionRepo;
  private readonly IWithdrawalRepository _withdrawalRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly AppDbContext _db;
  private readonly IAuditService _auditService;

  public WalletService(
    IWalletRepository walletRepo,
    IWalletTransactionRepository transactionRepo,
    IWithdrawalRepository withdrawalRepo,
    IBookingRepository bookingRepo,
    AppDbContext db,
    IAuditService auditService)
  {
    _walletRepo = walletRepo;
    _transactionRepo = transactionRepo;
    _withdrawalRepo = withdrawalRepo;
    _bookingRepo = bookingRepo;
    _db = db;
    _auditService = auditService;
  }

  public async Task<WalletDto> GetWalletAsync(Guid userId)
  {
    var wallet = await GetOrCreateWalletAsync(userId);
    return new WalletDto
    {
      Id = wallet.Id,
      Balance = wallet.Balance,
      PendingBalance = wallet.PendingBalance,
      Status = wallet.Status
    };
  }

  public async Task<List<WalletTransactionDto>> GetTransactionsAsync(Guid userId)
  {
    var wallet = await GetOrCreateWalletAsync(userId);
    var transactions = await _transactionRepo.FindByWalletIdAsync(wallet.Id);
    
    return transactions.Select(t => new WalletTransactionDto
    {
      Id = t.Id,
      Type = t.Type.ToString(),
      Amount = t.Amount,
      BalanceAfter = t.BalanceAfter,
      ReferenceId = t.ReferenceId,
      Description = t.Description,
      CreatedAt = t.CreatedAt
    }).ToList();
  }

  public async Task<List<WithdrawalDto>> GetWithdrawalsAsync(Guid userId)
  {
    var wallet = await GetOrCreateWalletAsync(userId);
    var withdrawals = await _withdrawalRepo.FindByWalletIdAsync(wallet.Id);

    return withdrawals.Select(w => new WithdrawalDto
    {
      Id = w.Id,
      Amount = w.Amount,
      BankName = w.BankName,
      AccountNumber = w.AccountNumber,
      AccountName = w.AccountName,
      Status = w.Status.ToString(),
      CreatedAt = w.CreatedAt
    }).ToList();
  }

  public async Task ProcessBookingIncomeAsync(Guid bookingId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found");

    if (booking.Status != BookingStatus.COMPLETED)
      throw new InvalidOperationException("Can only process income for COMPLETED bookings.");
      
    if (booking.WorkerId == null)
      throw new InvalidOperationException("Booking has no assigned worker.");

    // Prevent double processing. Check if transaction already exists
    var wallet = await GetOrCreateWalletAsync(booking.WorkerId.Value);
    var existingTransactions = await _transactionRepo.FindByWalletIdAsync(wallet.Id);
    if (existingTransactions.Any(t => t.ReferenceId == bookingId && t.Type == TransactionType.BOOKING_INCOME))
    {
      // Already processed
      return;
    }

    var totalAmount = booking.TotalAmount ?? 0;
    if (totalAmount <= 0) return;

    // Fixed commission rate: 10%
    var commissionRate = 0.1m;
    var commissionFee = totalAmount * commissionRate;
    var workerIncome = totalAmount - commissionFee;

    // Using EF transaction to ensure Ledger consistency
    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      // 1. Credit booking income
      var balanceBefore1 = wallet.Balance;
      var balanceAfter1 = wallet.Balance + totalAmount;
      wallet.Balance = balanceAfter1;
      
      await _transactionRepo.CreateAsync(new WalletTransaction
      {
        WalletId = wallet.Id,
        Type = TransactionType.BOOKING_INCOME,
        Amount = totalAmount,
        BalanceBefore = balanceBefore1,
        BalanceAfter = balanceAfter1,
        ReferenceId = booking.Id,
        Description = $"Booking income for {booking.Id}"
      });

      // 2. Debit commission fee
      var balanceBefore2 = wallet.Balance;
      var balanceAfter2 = wallet.Balance - commissionFee;
      wallet.Balance = balanceAfter2;

      await _transactionRepo.CreateAsync(new WalletTransaction
      {
        WalletId = wallet.Id,
        Type = TransactionType.COMMISSION_FEE,
        Amount = -commissionFee,
        BalanceBefore = balanceBefore2,
        BalanceAfter = balanceAfter2,
        ReferenceId = booking.Id,
        Description = "Platform commission fee (10%)"
      });

      await _walletRepo.UpdateAsync(wallet);
      await transaction.CommitAsync();

      await _auditService.LogActionAsync("WALLET_CREDIT", "Wallet", booking.WorkerId.Value, "WORKER", wallet.Id, null, $"Added {workerIncome} net income");
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task<WithdrawalDto> RequestWithdrawalAsync(Guid userId, WithdrawRequestDto request)
  {
    if (request.Amount <= 0)
      throw new ArgumentException("Withdrawal amount must be greater than zero.");

    var wallet = await GetOrCreateWalletAsync(userId);
    
    if (wallet.Balance < request.Amount)
      throw new InvalidOperationException("Insufficient balance.");

    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      var balanceBefore = wallet.Balance;
      var balanceAfter = wallet.Balance - request.Amount;
      
      // Deduct balance
      wallet.Balance = balanceAfter;
      // You could optionally add to PendingBalance here, but usually deductive is safer.
      
      await _transactionRepo.CreateAsync(new WalletTransaction
      {
        WalletId = wallet.Id,
        Type = TransactionType.WITHDRAWAL,
        Amount = -request.Amount,
        BalanceBefore = balanceBefore,
        BalanceAfter = balanceAfter,
        Description = "Withdrawal request"
      });

      var withdrawal = await _withdrawalRepo.CreateAsync(new Withdrawal
      {
        WalletId = wallet.Id,
        Amount = request.Amount,
        BankName = request.BankName,
        AccountNumber = request.AccountNumber,
        AccountName = request.AccountName,
        Status = WithdrawalStatus.PENDING
      });

      await _walletRepo.UpdateAsync(wallet);
      await transaction.CommitAsync();

      await _auditService.LogActionAsync("WITHDRAWAL_REQUEST", "Wallet", userId, "WORKER", withdrawal.Id, null, $"Requested {request.Amount}");

      return new WithdrawalDto
      {
        Id = withdrawal.Id,
        Amount = withdrawal.Amount,
        BankName = withdrawal.BankName,
        AccountNumber = withdrawal.AccountNumber,
        AccountName = withdrawal.AccountName,
        Status = withdrawal.Status.ToString(),
        CreatedAt = withdrawal.CreatedAt
      };
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  private async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
  {
    var wallet = await _walletRepo.FindByUserIdAsync(userId);
    if (wallet == null)
    {
      wallet = await _walletRepo.CreateAsync(new Wallet
      {
        UserId = userId,
        Balance = 0,
        PendingBalance = 0,
        Status = "ACTIVE"
      });
    }
    return wallet;
  }
}
