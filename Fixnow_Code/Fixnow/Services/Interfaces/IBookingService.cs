using Fixnow.DTOs.Booking;
using Fixnow.Enums;

namespace Fixnow.Services.Interfaces;

/// <summary>
/// Service interface for booking lifecycle operations.
/// </summary>
public interface IBookingService
{
  Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto request, Guid customerId);
  Task<BookingResponseDto> GetBookingAsync(Guid id, Guid requesterId);
  Task<List<BookingResponseDto>> GetMyBookingsAsync(Guid userId, UserRole role);
  Task<BookingResponseDto> AcceptBookingAsync(Guid bookingId, Guid workerId);
  Task<BookingResponseDto> RejectBookingAsync(Guid bookingId, Guid workerId);
  Task<BookingResponseDto> UpdateStatusAsync(Guid bookingId, Guid workerId, BookingStatus newStatus);
  Task<BookingResponseDto> CancelBookingAsync(Guid bookingId, Guid customerId);
}
