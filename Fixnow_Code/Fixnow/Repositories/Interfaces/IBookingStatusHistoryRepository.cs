using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IBookingStatusHistoryRepository
{
  Task AddAsync(BookingStatusHistory history);
}
