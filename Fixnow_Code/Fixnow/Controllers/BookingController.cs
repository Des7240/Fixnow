using System.Security.Claims;
using Fixnow.DTOs.Booking;
using Fixnow.Enums;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// Booking lifecycle controller: create, view, accept, reject, update status, cancel.
/// </summary>
[ApiController]
[Route("api/v1/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
  private readonly IBookingService _bookingService;

  public BookingController(IBookingService bookingService)
  {
    _bookingService = bookingService;
  }

  /// <summary>Create a new booking (CUSTOMER only).</summary>
  [HttpPost]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
  public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
  {
    var customerId = GetCurrentUserId();
    EnsureRole(UserRole.CUSTOMER);

    var result = await _bookingService.CreateBookingAsync(request, customerId);
    return StatusCode(StatusCodes.Status201Created, result);
  }

  /// <summary>Get booking detail by ID (owner only).</summary>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetBooking([FromRoute] Guid id)
  {
    var result = await _bookingService.GetBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Get current user's booking history.</summary>
  [HttpGet]
  [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMyBookings()
  {
    var role = GetCurrentUserRole();
    var result = await _bookingService.GetMyBookingsAsync(GetCurrentUserId(), role);
    return Ok(result);
  }

  /// <summary>Cancel a booking (CUSTOMER only – PENDING/MATCHING/ASSIGNED).</summary>
  [HttpPatch("{id:guid}/cancel")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> CancelBooking([FromRoute] Guid id)
  {
    EnsureRole(UserRole.CUSTOMER);
    var result = await _bookingService.CancelBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker accepts a booking (WORKER only).</summary>
  [HttpPost("{id:guid}/accept")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> AcceptBooking([FromRoute] Guid id)
  {
    EnsureRole(UserRole.WORKER);
    var result = await _bookingService.AcceptBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker rejects a booking (WORKER only).</summary>
  [HttpPost("{id:guid}/reject")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> RejectBooking([FromRoute] Guid id)
  {
    EnsureRole(UserRole.WORKER);
    var result = await _bookingService.RejectBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker updates booking status (ON_THE_WAY → WORKING → COMPLETED).</summary>
  [HttpPatch("{id:guid}/status")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateStatus(
    [FromRoute] Guid id,
    [FromBody] UpdateBookingStatusRequestDto request)
  {
    EnsureRole(UserRole.WORKER);
    var result = await _bookingService.UpdateStatusAsync(id, GetCurrentUserId(), request.Status);
    return Ok(result);
  }

  // ── Helpers ─────────────────────────────────────────────────────────────────

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue("sub")
      ?? throw new UnauthorizedAccessException("User ID not found in token.");
    return Guid.Parse(sub);
  }

  private UserRole GetCurrentUserRole()
  {
    var roleStr = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    return Enum.TryParse<UserRole>(roleStr, out var role) ? role : UserRole.CUSTOMER;
  }

  private void EnsureRole(UserRole required)
  {
    if (GetCurrentUserRole() != required)
      throw new UnauthorizedAccessException($"Only {required} can perform this action.");
  }
}
