using Fixnow.Data;
using Fixnow.Entities;
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
