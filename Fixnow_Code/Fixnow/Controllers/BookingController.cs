using System.Security.Claims;
using Fixnow.Data;
using Fixnow.DTOs.Booking;
using Fixnow.Enums;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

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
  private readonly AppDbContext _db;

  public BookingController(IBookingService bookingService, AppDbContext db)
  {
    _bookingService = bookingService;
    _db = db;
  }

  /// <summary>Create a new booking (CUSTOMER only).</summary>
  [HttpPost]
  [Authorize(Roles = "CUSTOMER")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
  public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto request)
  {
    var customerId = GetCurrentUserId();
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

  /// <summary>Get current worker's matching bookings (not yet accepted).</summary>
  [HttpGet("matching")]
  [Authorize(Roles = "WORKER")]
  [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMatchingBookings()
  {
    var result = await _bookingService.GetMatchingBookingsAsync(GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Cancel a booking (CUSTOMER only – PENDING/MATCHING/ASSIGNED).</summary>
  [HttpPatch("{id:guid}/cancel")]
  [Authorize(Roles = "CUSTOMER")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> CancelBooking([FromRoute] Guid id)
  {
    var result = await _bookingService.CancelBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker accepts a booking (WORKER only).</summary>
  [HttpPost("{id:guid}/accept")]
  [Authorize(Roles = "WORKER")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> AcceptBooking([FromRoute] Guid id)
  {
    var result = await _bookingService.AcceptBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker rejects a booking (WORKER only).</summary>
  [HttpPost("{id:guid}/reject")]
  [Authorize(Roles = "WORKER")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> RejectBooking([FromRoute] Guid id)
  {
    var result = await _bookingService.RejectBookingAsync(id, GetCurrentUserId());
    return Ok(result);
  }

  /// <summary>Worker updates booking status (ON_THE_WAY → WORKING → COMPLETED).</summary>
  [HttpPatch("{id:guid}/status")]
  [Authorize(Roles = "WORKER")]
  [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateStatus(
    [FromRoute] Guid id,
    [FromBody] UpdateBookingStatusRequestDto request)
  {
    var result = await _bookingService.UpdateStatusAsync(id, GetCurrentUserId(), request.Status);
    return Ok(result);
  }

  // ── Helpers ─────────────────────────────────────────────────────────────────

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
      ?? throw new UnauthorizedAccessException("User ID not found in token.");
    return Guid.Parse(sub);
  }

  private UserRole GetCurrentUserRole()
  {
    var roleStr = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    return Enum.TryParse<UserRole>(roleStr, out var role) ? role : UserRole.CUSTOMER;
  }

  /// <summary>GET /api/v1/bookings/{id}/timeline — booking event history.</summary>
  [HttpGet("{id:guid}/timeline")]
  public async Task<ActionResult<IList<BookingTimelineDto>>> GetTimeline(Guid id)
  {
    var userId = GetCurrentUserId();

    // Access control:
    // 1. Customer who created it
    // 2. Worker who is assigned
    // 3. Worker who was notified for matching
    var booking = await _db.Bookings
      .Include(b => b.MatchingLogs)
      .FirstOrDefaultAsync(b => b.Id == id);

    if (booking == null)
      return NotFound(new { message = "Booking not found." });

    var isNotifiedWorker = booking.Status == BookingStatus.MATCHING && 
                           booking.MatchingLogs.Any(l => l.WorkerId == userId && l.Status == MatchingLogStatus.NOTIFIED);

    if (booking.CustomerId != userId && booking.WorkerId != userId && !isNotifiedWorker)
      return Forbid();

    var history = await _db.BookingStatusHistories
      .Where(h => h.BookingId == id)
      .OrderBy(h => h.CreatedAt)
      .Select(h => new BookingTimelineDto
      {
        Id = h.Id,
        OldStatus = h.OldStatus.HasValue ? h.OldStatus.ToString() : null,
        NewStatus = h.NewStatus.ToString(),
        CreatedAt = h.CreatedAt
      })
      .ToListAsync();

    return Ok(history);
  }
}
