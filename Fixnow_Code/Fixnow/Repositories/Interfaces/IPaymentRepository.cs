using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IPaymentRepository
{
  Task<Payment> CreateAsync(Payment payment);
  Task<Payment?> FindByIdAsync(Guid id);
  Task<Payment?> FindByIdWithDetailsAsync(Guid id);
  Task UpdateAsync(Payment payment);
  Task AddTransactionAsync(Transaction transaction);
  Task AddCallbackAsync(PaymentCallback callback);
  Task<BookingFinancial> CreateFinancialAsync(BookingFinancial financial);
}
