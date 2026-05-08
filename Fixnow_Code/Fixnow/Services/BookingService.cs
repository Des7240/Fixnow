using Fixnow.DTOs.Booking;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace Fixnow.Services;

/// <summary>
/// Core booking lifecycle service: create, accept, reject, update status, cancel.
/// </summary>
public class BookingService : IBookingService
{
  // Valid transitions that a WORKER can trigger
  private static readonly Dictionary<BookingStatus, BookingStatus[]> WorkerTransitions = new()
  {
    [BookingStatus.ASSIGNED] = new[] { BookingStatus.ON_THE_WAY },
    [BookingStatus.ON_THE_WAY] = new[] { BookingStatus.WORKING },
    [BookingStatus.WORKING] = new[] { BookingStatus.COMPLETED },
  };

  private static readonly GeometryFactory GeomFactory = new(new PrecisionModel(), 4326);

  private readonly IBookingRepository _bookingRepo;
  private readonly IBookingMatchingLogRepository _matchingLogRepo;
  private readonly IServiceCategoryRepository _serviceRepo;
  private readonly IMatchingService _matchingService;
  private readonly INotificationService _notificationService;

  public BookingService(
    IBookingRepository bookingRepo,
    IBookingMatchingLogRepository matchingLogRepo,
    IServiceCategoryRepository serviceRepo,
    IMatchingService matchingService,
    INotificationService notificationService)
  {
    _bookingRepo = bookingRepo;
    _matchingLogRepo = matchingLogRepo;
    _serviceRepo = serviceRepo;
    _matchingService = matchingService;
    _notificationService = notificationService;
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto request, Guid customerId)
  {
    var service = await _serviceRepo.FindByIdAsync(request.ServiceId)
      ?? throw new KeyNotFoundException("Service not found.");

    var location = GeomFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));

    var booking = new Booking
    {
      CustomerId = customerId,
      ServiceId = request.ServiceId,
      Address = request.Address,
      Lat = request.Lat,
      Lng = request.Lng,
      Location = location,
      Description = request.Description,
      Status = BookingStatus.PENDING,
    };

    await _bookingRepo.CreateAsync(booking);

    // Trigger matching asynchronously (synchronous for MVP simplicity)
    await _matchingService.TriggerMatchingAsync(booking.Id);

    // Reload with full details
    var created = await _bookingRepo.FindByIdWithDetailsAsync(booking.Id)
      ?? throw new InvalidOperationException("Failed to reload booking.");

    return MapToDto(created);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> GetBookingAsync(Guid id, Guid requesterId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(id)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.CustomerId != requesterId && booking.WorkerId != requesterId)
      throw new UnauthorizedAccessException("Access denied.");

    return MapToDto(booking);
  }

  /// <inheritdoc/>
  public async Task<List<BookingResponseDto>> GetMyBookingsAsync(Guid userId, UserRole role)
  {
    var bookings = role == UserRole.CUSTOMER
      ? await _bookingRepo.FindByCustomerAsync(userId)
      : await _bookingRepo.FindByWorkerAsync(userId);

    return bookings.Select(MapToDto).ToList();
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> AcceptBookingAsync(Guid bookingId, Guid workerId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.Status != BookingStatus.MATCHING)
      throw new InvalidOperationException($"Cannot accept booking with status '{booking.Status}'.");

    // Verify worker was notified for this booking
    var log = await _matchingLogRepo.FindByBookingAndWorkerAsync(bookingId, workerId)
      ?? throw new UnauthorizedAccessException("Worker was not notified for this booking.");

    if (log.Status != MatchingLogStatus.NOTIFIED)
      throw new InvalidOperationException("Worker has already responded to this booking.");

    // Assign worker and update status
    booking.WorkerId = workerId;
    booking.Status = BookingStatus.ASSIGNED;
    await _bookingRepo.UpdateAsync(booking);

    // Update matching logs
    await _matchingLogRepo.UpdateStatusAsync(log, MatchingLogStatus.ACCEPTED);
    await _matchingLogRepo.ExpireAllNotifiedAsync(bookingId);

    // Notify customer
    await _notificationService.NotifyCustomerBookingAssignedAsync(
      booking.CustomerId, bookingId, booking.Worker?.FullName ?? "Worker");

    return MapToDto(booking);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> RejectBookingAsync(Guid bookingId, Guid workerId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.Status != BookingStatus.MATCHING)
      throw new InvalidOperationException($"Cannot reject booking with status '{booking.Status}'.");

    var log = await _matchingLogRepo.FindByBookingAndWorkerAsync(bookingId, workerId)
      ?? throw new UnauthorizedAccessException("Worker was not notified for this booking.");

    await _matchingLogRepo.UpdateStatusAsync(log, MatchingLogStatus.REJECTED);

    // Check if any other workers are still NOTIFIED
    var remaining = await _matchingLogRepo.FindNotifiedByBookingAsync(bookingId);

    if (remaining.Count == 0)
    {
      // No more candidates – reset to PENDING and re-trigger matching
      booking.Status = BookingStatus.PENDING;
      await _bookingRepo.UpdateAsync(booking);
      await _matchingService.TriggerMatchingAsync(booking.Id);

      // Reload after re-trigger
      booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
        ?? throw new InvalidOperationException("Failed to reload booking.");
    }

    return MapToDto(booking);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> UpdateStatusAsync(Guid bookingId, Guid workerId, BookingStatus newStatus)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.WorkerId != workerId)
      throw new UnauthorizedAccessException("Only the assigned worker can update status.");

    if (!WorkerTransitions.TryGetValue(booking.Status, out var allowed)
      || !allowed.Contains(newStatus))
    {
      throw new InvalidOperationException(
        $"Invalid transition: '{booking.Status}' → '{newStatus}'.");
    }

    booking.Status = newStatus;
    await _bookingRepo.UpdateAsync(booking);
    await _notificationService.NotifyCustomerBookingStatusAsync(
      booking.CustomerId, bookingId, newStatus.ToString());

    return MapToDto(booking);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> CancelBookingAsync(Guid bookingId, Guid customerId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.CustomerId != customerId)
      throw new UnauthorizedAccessException("Only the booking owner can cancel.");

    var cancellableStatuses = new[] { BookingStatus.PENDING, BookingStatus.MATCHING, BookingStatus.ASSIGNED };

    if (!cancellableStatuses.Contains(booking.Status))
      throw new InvalidOperationException($"Cannot cancel booking with status '{booking.Status}'.");

    booking.Status = BookingStatus.CANCELLED;
    await _bookingRepo.UpdateAsync(booking);

    return MapToDto(booking);
  }

  /// <summary>Maps a Booking entity to BookingResponseDto.</summary>
  private static BookingResponseDto MapToDto(Booking booking)
  {
    return new BookingResponseDto
    {
      Id = booking.Id,
      Status = booking.Status.ToString(),
      Address = booking.Address,
      Lat = booking.Lat,
      Lng = booking.Lng,
      Description = booking.Description,
      CreatedAt = booking.CreatedAt,
      Customer = new BookingPartyDto
      {
        Id = booking.Customer?.Id ?? booking.CustomerId,
        FullName = booking.Customer?.FullName ?? string.Empty,
        Email = booking.Customer?.Email ?? string.Empty,
      },
      Worker = booking.Worker is null ? null : new BookingPartyDto
      {
        Id = booking.Worker.Id,
        FullName = booking.Worker.FullName,
        Email = booking.Worker.Email,
      },
      Service = new BookingServiceDto
      {
        Id = booking.Service?.Id ?? booking.ServiceId,
        Name = booking.Service?.Name ?? string.Empty,
      },
    };
  }
}
