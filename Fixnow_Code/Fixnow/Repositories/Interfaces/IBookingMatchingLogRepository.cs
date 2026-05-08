using Fixnow.Entities;
using Fixnow.Enums;

namespace Fixnow.Repositories.Interfaces;

/// <summary>
/// Repository interface for booking matching log operations.
/// </summary>
public interface IBookingMatchingLogRepository
{
  Task<BookingMatchingLog> CreateAsync(BookingMatchingLog log);
  Task<BookingMatchingLog?> FindByBookingAndWorkerAsync(Guid bookingId, Guid workerId);
  Task<List<BookingMatchingLog>> FindNotifiedByBookingAsync(Guid bookingId);
  Task UpdateStatusAsync(BookingMatchingLog log, MatchingLogStatus status);
  Task ExpireAllNotifiedAsync(Guid bookingId);
}
