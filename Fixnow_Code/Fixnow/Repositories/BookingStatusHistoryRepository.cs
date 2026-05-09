using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;

namespace Fixnow.Repositories;

public class BookingStatusHistoryRepository : IBookingStatusHistoryRepository
{
  private readonly AppDbContext _db;

  public BookingStatusHistoryRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task AddAsync(BookingStatusHistory history)
  {
    _db.BookingStatusHistories.Add(history);
    await _db.SaveChangesAsync();
  }
}
