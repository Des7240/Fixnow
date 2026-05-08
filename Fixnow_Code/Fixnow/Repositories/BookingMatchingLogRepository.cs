using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IBookingMatchingLogRepository.
/// </summary>
public class BookingMatchingLogRepository : IBookingMatchingLogRepository
{
  private readonly AppDbContext _context;

  public BookingMatchingLogRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<BookingMatchingLog> CreateAsync(BookingMatchingLog log)
  {
    _context.BookingMatchingLogs.Add(log);
    await _context.SaveChangesAsync();
    return log;
  }

  /// <inheritdoc/>
  public async Task<BookingMatchingLog?> FindByBookingAndWorkerAsync(Guid bookingId, Guid workerId)
  {
    return await _context.BookingMatchingLogs
      .FirstOrDefaultAsync(l => l.BookingId == bookingId && l.WorkerId == workerId);
  }

  /// <inheritdoc/>
  public async Task<List<BookingMatchingLog>> FindNotifiedByBookingAsync(Guid bookingId)
  {
    return await _context.BookingMatchingLogs
      .Where(l => l.BookingId == bookingId && l.Status == MatchingLogStatus.NOTIFIED)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task UpdateStatusAsync(BookingMatchingLog log, MatchingLogStatus status)
  {
    log.Status = status;
    await _context.SaveChangesAsync();
  }

  /// <inheritdoc/>
  public async Task ExpireAllNotifiedAsync(Guid bookingId)
  {
    var logs = await FindNotifiedByBookingAsync(bookingId);
    logs.ForEach(l => l.Status = MatchingLogStatus.EXPIRED);
    await _context.SaveChangesAsync();
  }
}
