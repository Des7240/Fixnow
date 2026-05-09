using Fixnow.Entities;
using Fixnow.Enums;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for Booking entity operations.
/// </summary>
public interface IBookingRepository
{
  Task<Booking?> FindByIdAsync(Guid id);
  Task<Booking?> FindByIdWithDetailsAsync(Guid id);
  Task<List<Booking>> FindByCustomerAsync(Guid customerId);
  Task<List<Booking>> FindByWorkerAsync(Guid workerId);
  Task<List<Booking>> FindMatchingByWorkerAsync(Guid workerId);
  Task<Booking> CreateAsync(Booking booking);
  Task UpdateAsync(Booking booking);
}
