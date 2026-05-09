using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class PaymentRepository : IPaymentRepository
{
  private readonly AppDbContext _db;

  public PaymentRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<Payment> CreateAsync(Payment payment)
  {
    _db.Payments.Add(payment);
    await _db.SaveChangesAsync();
    return payment;
  }

  public async Task<Payment?> FindByIdAsync(Guid id)
  {
    return await _db.Payments.FindAsync(id);
  }

  public async Task<Payment?> FindByIdWithDetailsAsync(Guid id)
  {
    return await _db.Payments
      .Include(p => p.Booking)
      .Include(p => p.Customer)
      .Include(p => p.Transactions)
      .FirstOrDefaultAsync(p => p.Id == id);
  }

  public async Task UpdateAsync(Payment payment)
  {
    payment.UpdatedAt = DateTime.UtcNow;
    _db.Payments.Update(payment);
    await _db.SaveChangesAsync();
  }

  public async Task AddTransactionAsync(Transaction transaction)
  {
    _db.Transactions.Add(transaction);
    await _db.SaveChangesAsync();
  }

  public async Task AddCallbackAsync(PaymentCallback callback)
  {
    _db.PaymentCallbacks.Add(callback);
    await _db.SaveChangesAsync();
  }

  public async Task<BookingFinancial> CreateFinancialAsync(BookingFinancial financial)
  {
    _db.BookingFinancials.Add(financial);
    await _db.SaveChangesAsync();
    return financial;
  }
}
