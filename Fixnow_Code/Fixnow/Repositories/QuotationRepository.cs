using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

public class QuotationRepository : IQuotationRepository
{
  private readonly AppDbContext _db;

  public QuotationRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<Quotation> CreateAsync(Quotation quotation)
  {
    _db.Quotations.Add(quotation);
    await _db.SaveChangesAsync();
    return quotation;
  }

  public async Task<Quotation?> FindByIdAsync(Guid id)
  {
    return await _db.Quotations.FindAsync(id);
  }

  public async Task<Quotation?> FindByIdWithDetailsAsync(Guid id)
  {
    return await _db.Quotations
      .Include(q => q.Items)
      .Include(q => q.Worker)
      .Include(q => q.Customer)
      .Include(q => q.Booking)
      .FirstOrDefaultAsync(q => q.Id == id);
  }

  public async Task<List<Quotation>> FindByBookingIdAsync(Guid bookingId)
  {
    return await _db.Quotations
      .Include(q => q.Items)
      .Include(q => q.Worker)
      .Include(q => q.Customer)
      .Where(q => q.BookingId == bookingId)
      .OrderByDescending(q => q.CreatedAt)
      .ToListAsync();
  }

  public async Task UpdateAsync(Quotation quotation)
  {
    _db.Quotations.Update(quotation);
    await _db.SaveChangesAsync();
  }
}
