using Fixnow.DTOs.Wallet;

namespace Fixnow.Services.Interfaces;

public interface IWalletService
{
  Task<WalletDto> GetWalletAsync(Guid userId);
  Task<List<WalletTransactionDto>> GetTransactionsAsync(Guid userId);
  Task<List<WithdrawalDto>> GetWithdrawalsAsync(Guid userId);
  
  Task ProcessBookingIncomeAsync(Guid bookingId);
  Task<WithdrawalDto> RequestWithdrawalAsync(Guid userId, WithdrawRequestDto request);
}
