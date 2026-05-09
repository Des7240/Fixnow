using Fixnow.Data;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Repositories;

/// <summary>
/// EF Core implementation of IBookingRepository.
/// </summary>
public class BookingRepository : IBookingRepository
{
  private readonly AppDbContext _context;

  public BookingRepository(AppDbContext context)
  {
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<Booking?> FindByIdAsync(Guid id)
  {
    return await _context.Bookings.FindAsync(id);
  }

  /// <inheritdoc/>
  public async Task<Booking?> FindByIdWithDetailsAsync(Guid id)
  {
    return await _context.Bookings
      .Include(b => b.Customer)
      .Include(b => b.Worker)
      .Include(b => b.Service)
      .Include(b => b.Quotations)
        .ThenInclude(q => q.Items)
      .FirstOrDefaultAsync(b => b.Id == id);
  }

  /// <inheritdoc/>
  public async Task<List<Booking>> FindByCustomerAsync(Guid customerId)
  {
    return await _context.Bookings
      .Include(b => b.Service)
      .Include(b => b.Worker)
      .Where(b => b.CustomerId == customerId)
      .OrderByDescending(b => b.CreatedAt)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<List<Booking>> FindByWorkerAsync(Guid workerId)
  {
    return await _context.Bookings
      .Include(b => b.Service)
      .Include(b => b.Customer)
      .Where(b => b.WorkerId == workerId)
      .OrderByDescending(b => b.CreatedAt)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<List<Booking>> FindMatchingByWorkerAsync(Guid workerId)
  {
    // 1. Get worker's current location and skills
    var workerLoc = await _context.WorkerLocations.FindAsync(workerId);
    var workerSkills = await _context.WorkerServices
      .Where(ws => ws.WorkerId == workerId)
      .Select(ws => ws.ServiceId)
      .ToListAsync();

    if (workerLoc == null || !workerSkills.Any())
    {
      // Fallback to logs if location/skills not available
      return await _context.Bookings
        .Include(b => b.Service)
        .Include(b => b.Customer)
        .Where(b => b.Status == BookingStatus.MATCHING && _context.BookingMatchingLogs
          .Any(l => l.BookingId == b.Id && l.WorkerId == workerId))
        .OrderByDescending(b => b.CreatedAt)
        .ToListAsync();
    }

    // 2. Find MATCHING bookings that are nearby (10km) and match skills
    // and where the worker hasn't been rejected or hasn't accepted yet
    return await _context.Bookings
      .Include(b => b.Service)
      .Include(b => b.Customer)
      .Where(b => b.Status == BookingStatus.MATCHING)
      .Where(b => workerSkills.Contains(b.ServiceId))
      .Where(b => b.Location.IsWithinDistance(workerLoc.Location, 10000)) // 10km
      .OrderByDescending(b => b.CreatedAt)
      .ToListAsync();
  }

  /// <inheritdoc/>
  public async Task<Booking> CreateAsync(Booking booking)
  {
    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync();
    return booking;
  }

  /// <inheritdoc/>
  public async Task UpdateAsync(Booking booking)
  {
    booking.UpdatedAt = DateTime.UtcNow;
    _context.Bookings.Update(booking);
    await _context.SaveChangesAsync();
  }
}
