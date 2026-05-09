namespace Fixnow.Services.Interfaces;

public interface IBookingJobService
{
  Task CancelExpiredBookingAsync(Guid bookingId);
}
