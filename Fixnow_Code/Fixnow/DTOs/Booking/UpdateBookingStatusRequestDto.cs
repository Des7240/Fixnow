using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.Booking;

/// <summary>
/// DTO for worker to update a booking's status.
/// Only valid worker-driven transitions are allowed.
/// </summary>
public class UpdateBookingStatusRequestDto
{
  [Required]
  public BookingStatus Status { get; set; }
}
