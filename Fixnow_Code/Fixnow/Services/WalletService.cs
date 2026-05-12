using Fixnow.Data;
using Fixnow.DTOs.Wallet;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

public class WalletService : IWalletService
{
  private readonly IWalletRepository _walletRepo;
  private readonly IWalletTransactionRepository _transactionRepo;
  private readonly IWithdrawalRepository _withdrawalRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly AppDbContext _db;
  private readonly IAuditService _auditService;
  private readonly IOtpService _otpService;
  private readonly IUserRepository _userRepo;

  public WalletService(
    IWalletRepository walletRepo,
    IWalletTransactionRepository transactionRepo,
    IWithdrawalRepository withdrawalRepo,
    IBookingRepository bookingRepo,
    AppDbContext db,
    IAuditService auditService,
    IOtpService otpService,
    IUserRepository userRepo)
  {
    _walletRepo = walletRepo;
    _transactionRepo = transactionRepo;
    _withdrawalRepo = withdrawalRepo;
    _bookingRepo = bookingRepo;
    _db = db;
    _auditService = auditService;
    _otpService = otpService;
    _userRepo = userRepo;
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

    if (booking.Status != BookingStatus.WORKING && booking.Status != BookingStatus.COMPLETED)
      throw new InvalidOperationException("Can only process income for WORKING or COMPLETED bookings.");
      
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

    // Get dynamic commission rate for this service
    var commissionConfig = await _db.ServiceCommissions
        .FirstOrDefaultAsync(c => c.ServiceId == booking.ServiceId && c.IsActive);
    
    var commissionRate = commissionConfig?.CommissionPercent / 100m ?? 0.1m; // Default 10%
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
        Description = $"Platform commission fee ({commissionRate * 100}%)"
      });

      await _walletRepo.UpdateAsync(wallet);
      await transaction.CommitAsync();

      await _auditService.LogActionAsync("WALLET_CREDIT", "Wallet", booking.WorkerId.Value, "WORKER", wallet.Id, null, $"Added {workerIncome} net income (Commission: {commissionRate * 100}%)");
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task ProcessDepositAsync(Guid paymentId)
  {
    var payment = await _db.Payments
      .Include(p => p.Customer)
      .FirstOrDefaultAsync(p => p.Id == paymentId)
      ?? throw new KeyNotFoundException("Payment not found");

    if (payment.Status != PaymentStatus.SUCCESS)
      throw new InvalidOperationException("Payment is not successful.");

    if (payment.Type != PaymentType.WALLET_DEPOSIT)
      throw new InvalidOperationException("Payment is not a wallet deposit.");

    var wallet = await GetOrCreateWalletAsync(payment.CustomerId);

    // Prevent double processing
    var existingTransactions = await _transactionRepo.FindByWalletIdAsync(wallet.Id);
    if (existingTransactions.Any(t => t.ReferenceId == paymentId && t.Type == TransactionType.DEPOSIT))
    {
      return;
    }

    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      var balanceBefore = wallet.Balance;
      var balanceAfter = wallet.Balance + payment.Amount;
      wallet.Balance = balanceAfter;

      await _transactionRepo.CreateAsync(new WalletTransaction
      {
        WalletId = wallet.Id,
        Type = TransactionType.DEPOSIT,
        Amount = payment.Amount,
        BalanceBefore = balanceBefore,
        BalanceAfter = balanceAfter,
        ReferenceId = paymentId,
        Description = "Wallet top-up deposit"
      });

      await _walletRepo.UpdateAsync(wallet);
      await transaction.CommitAsync();

      await _auditService.LogActionAsync("WALLET_DEPOSIT", "Wallet", payment.CustomerId, "USER", wallet.Id, null, $"Deposited {payment.Amount}");
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task InitiateWithdrawalAsync(Guid userId, WithdrawRequestDto request)
  {
    if (request.Amount <= 0)
      throw new ArgumentException("Số tiền rút phải lớn hơn 0.");

    var minAmountConfig = await _db.SystemConfigs.FindAsync("MIN_WITHDRAW_AMOUNT");
    var maxAmountConfig = await _db.SystemConfigs.FindAsync("MAX_WITHDRAW_AMOUNT");
    var dailyLimitConfig = await _db.SystemConfigs.FindAsync("DAILY_WITHDRAW_LIMIT");

    decimal minAmount = decimal.Parse(minAmountConfig?.ConfigValue ?? "50000");
    decimal maxAmount = decimal.Parse(maxAmountConfig?.ConfigValue ?? "20000000");
    decimal dailyLimit = decimal.Parse(dailyLimitConfig?.ConfigValue ?? "50000000");

    if (request.Amount < minAmount)
        throw new InvalidOperationException($"Số tiền rút tối thiểu là {minAmount:N0} VNĐ.");
    if (request.Amount > maxAmount)
        throw new InvalidOperationException($"Số tiền rút tối đa một lần là {maxAmount:N0} VNĐ.");

    var wallet = await GetOrCreateWalletAsync(userId);

    // Check daily limit
    var todayStart = DateTime.UtcNow.Date;
    var todayWithdrawalsTotal = await _db.Withdrawals
        .Where(w => w.WalletId == wallet.Id && w.CreatedAt >= todayStart && w.Status != WithdrawalStatus.REJECTED)
        .SumAsync(w => w.Amount);

    if (todayWithdrawalsTotal + request.Amount > dailyLimit)
        throw new InvalidOperationException($"Bạn đã vượt quá hạn mức rút tiền trong ngày ({dailyLimit:N0} VNĐ).");

    if (wallet.Balance < request.Amount)
      throw new InvalidOperationException("Số dư không đủ.");

    var user = await _userRepo.FindByIdAsync(userId)
      ?? throw new KeyNotFoundException("User not found.");

    await _otpService.GenerateOtpAsync(user.Email, OtpType.WITHDRAWAL_VERIFICATION, "Xác nhận rút tiền");
    
    await _auditService.LogActionAsync("WITHDRAWAL_INITIATED", "Wallet", userId, "WORKER", wallet.Id, null, $"Requested OTP for withdrawal of {request.Amount}");
  }

  public async Task<WithdrawalDto> ConfirmWithdrawalAsync(Guid userId, ConfirmWithdrawRequestDto request)
  {
    var user = await _userRepo.FindByIdAsync(userId)
      ?? throw new KeyNotFoundException("User not found.");

    var isValid = await _otpService.VerifyOtpAsync(user.Email, request.OtpCode, OtpType.WITHDRAWAL_VERIFICATION);
    if (!isValid)
      throw new InvalidOperationException("Mã xác thực không chính xác hoặc đã hết hạn.");

    var wallet = await GetOrCreateWalletAsync(userId);
    if (wallet.Balance < request.Amount)
      throw new InvalidOperationException("Số dư không đủ tại thời điểm xác nhận.");

    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      var balanceBefore = wallet.Balance;
      var balanceAfter = wallet.Balance - request.Amount;

      wallet.Balance = balanceAfter;

      await _transactionRepo.CreateAsync(new WalletTransaction
      {
        WalletId = wallet.Id,
        Type = TransactionType.WITHDRAWAL,
        Amount = -request.Amount,
        BalanceBefore = balanceBefore,
        BalanceAfter = balanceAfter,
        Description = "Withdrawal request confirmed"
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

      await _auditService.LogActionAsync("WITHDRAWAL_CONFIRMED", "Wallet", userId, "WORKER", withdrawal.Id, null, $"Withdrawal of {request.Amount} confirmed");

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
