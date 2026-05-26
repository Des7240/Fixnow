using Fixnow.Data;
using Fixnow.DTOs.Booking;
using Fixnow.DTOs.Quotation;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Fixnow.Services;

/// <summary>
/// Core booking lifecycle service: create, accept, reject, update status, cancel.
/// </summary>
public class BookingService : IBookingService
{
  private readonly IBookingRepository _bookingRepo;
  private readonly IServiceCategoryRepository _serviceRepo;
  private readonly IMatchingService _matchingService;
  private readonly INotificationService _notificationService;
  private readonly IBookingStatusHistoryRepository _historyRepo;
  private readonly IAuditService _auditService;
  private readonly IConversationRepository _conversationRepo;
  private readonly IBackgroundJobClient _backgroundJobClient;
  private readonly IBookingMatchingLogRepository _matchingLogRepo;
  private readonly IUserRepository _userRepo;
  private readonly AppDbContext _context;

  private static readonly GeometryFactory GeomFactory = new(new PrecisionModel(), 4326);

  public BookingService(
    IBookingRepository bookingRepo,
    IServiceCategoryRepository serviceRepo,
    IMatchingService matchingService,
    INotificationService notificationService,
    IBookingStatusHistoryRepository historyRepo,
    IAuditService auditService,
    IConversationRepository conversationRepo,
    IBookingMatchingLogRepository matchingLogRepo,
    IUserRepository userRepo,
    IBackgroundJobClient backgroundJobClient,
    AppDbContext context)
  {
    _bookingRepo = bookingRepo;
    _serviceRepo = serviceRepo;
    _matchingService = matchingService;
    _notificationService = notificationService;
    _historyRepo = historyRepo;
    _auditService = auditService;
    _conversationRepo = conversationRepo;
    _matchingLogRepo = matchingLogRepo;
    _userRepo = userRepo;
    _backgroundJobClient = backgroundJobClient;
    _context = context;
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingRequestDto request, Guid customerId)
  {
    var user = await _userRepo.FindByIdAsync(customerId)
      ?? throw new KeyNotFoundException("Không tìm thấy thông tin người dùng.");

    if (string.IsNullOrEmpty(user.PhoneNumber))
    {
        throw new InvalidOperationException("Bạn cần cập nhật số điện thoại trong hồ sơ trước khi thực hiện đặt thợ.");
    }

    if (user.NeedsPasswordReset)
    {
        throw new InvalidOperationException("Bạn cần thiết lập mật khẩu cho tài khoản trước khi thực hiện giao dịch này.");
    }

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

    // Log status history
    await LogStatusChangeAsync(booking.Id, null, BookingStatus.PENDING, customerId);

    // Trigger matching
    await _matchingService.TriggerMatchingAsync(booking.Id);

    // Schedule auto cancel if not accepted within 5 minutes
    _backgroundJobClient.Schedule<IBookingJobService>(
        x => x.CancelExpiredBookingAsync(booking.Id), 
        TimeSpan.FromMinutes(5));

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

    // Access control:
    // 1. Customer who created it
    // 2. Worker who is assigned
    // 3. Worker who is eligible for matching (status is MATCHING, has required skill, and is nearby)
    var isEligibleWorker = false;
    if (booking.Status == BookingStatus.MATCHING)
    {
      var workerLoc = await _context.WorkerLocations.FindAsync(requesterId);
      var hasSkill = await _context.WorkerServices.AnyAsync(ws => ws.WorkerId == requesterId && ws.ServiceId == booking.ServiceId && ws.Status == Enums.WorkerServiceStatus.APPROVED);
      
      var isNearby = workerLoc != null && workerLoc.Location.IsWithinDistance(booking.Location, 10000); // 10km
      isEligibleWorker = hasSkill && isNearby;
    }

    if (booking.CustomerId != requesterId && booking.WorkerId != requesterId && !isEligibleWorker)
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
  public async Task<List<BookingResponseDto>> GetMatchingBookingsAsync(Guid workerId)
  {
    var bookings = await _bookingRepo.FindMatchingByWorkerAsync(workerId);
    return bookings.Select(MapToDto).ToList();
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> AcceptBookingAsync(Guid bookingId, Guid workerId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.Status != BookingStatus.MATCHING)
      throw new InvalidOperationException($"Cannot accept booking with status '{booking.Status}'.");

    // Verify worker eligibility (Proximity + Skills)
    var workerLoc = await _context.WorkerLocations.FindAsync(workerId);
    var hasSkill = await _context.WorkerServices.AnyAsync(ws => ws.WorkerId == workerId && ws.ServiceId == booking.ServiceId && ws.Status == Enums.WorkerServiceStatus.APPROVED);
    var isNearby = workerLoc != null && workerLoc.Location.IsWithinDistance(booking.Location, 10000);

    if (!hasSkill || !isNearby)
      throw new UnauthorizedAccessException("You are not eligible to accept this booking (location or skills mismatch).");

    var oldStatus = booking.Status;

    // Assign worker and update status
    booking.WorkerId = workerId;
    booking.Status = BookingStatus.ASSIGNED;
    await _bookingRepo.UpdateAsync(booking);

    // Log status history
    await LogStatusChangeAsync(bookingId, oldStatus, BookingStatus.ASSIGNED, workerId);

    // Update matching logs
    var log = await _matchingLogRepo.FindByBookingAndWorkerAsync(bookingId, workerId);
    if (log != null)
    {
      await _matchingLogRepo.UpdateStatusAsync(log, MatchingLogStatus.ACCEPTED);
    }
    else
    {
      await _matchingLogRepo.CreateAsync(new BookingMatchingLog
      {
        BookingId = bookingId,
        WorkerId = workerId,
        Status = MatchingLogStatus.ACCEPTED,
        DistanceMeters = workerLoc!.Location.Distance(booking.Location)
      });
    }

    await _matchingLogRepo.ExpireAllNotifiedAsync(bookingId);

    // Create chat conversation
    await _conversationRepo.CreateAsync(new Conversation
    {
      BookingId = bookingId,
      CustomerId = booking.CustomerId,
      WorkerId = workerId
    });

    // Notify customer
    var worker = await _userRepo.FindByIdAsync(workerId);
    await _notificationService.NotifyCustomerBookingAssignedAsync(
      booking.CustomerId, bookingId, worker?.FullName ?? "Thợ");

    return MapToDto(booking);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> RejectBookingAsync(Guid bookingId, Guid workerId)
  {
    var log = await _matchingLogRepo.FindByBookingAndWorkerAsync(bookingId, workerId)
      ?? throw new KeyNotFoundException("Matching notification not found.");

    await _matchingLogRepo.UpdateStatusAsync(log, MatchingLogStatus.REJECTED);
    
    var booking = await _bookingRepo.FindByIdAsync(bookingId);
    return MapToDto(booking!);
  }

  /// <inheritdoc/>
  public async Task<BookingResponseDto> UpdateStatusAsync(Guid bookingId, Guid workerId, BookingStatus newStatus)
  {
    var booking = await _bookingRepo.FindByIdAsync(bookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.WorkerId != workerId)
      throw new UnauthorizedAccessException("Only the assigned worker can update status.");

    var oldStatus = booking.Status;
    booking.Status = newStatus;
    await _bookingRepo.UpdateAsync(booking);

    // Log status history
    await LogStatusChangeAsync(bookingId, oldStatus, newStatus, workerId);

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

    var oldStatus = booking.Status;
    booking.Status = BookingStatus.CANCELLED;
    await _bookingRepo.UpdateAsync(booking);

    // Log status history
    await LogStatusChangeAsync(bookingId, oldStatus, BookingStatus.CANCELLED, customerId);

    // Notify worker if already assigned
    if (booking.WorkerId.HasValue)
    {
      await _notificationService.NotifyWorkerBookingStatusAsync(
        booking.WorkerId.Value, bookingId, "CANCELLED");
    }

    return MapToDto(booking);
  }

  private async Task LogStatusChangeAsync(Guid bookingId, BookingStatus? oldStatus, BookingStatus newStatus, Guid updatedBy)
  {
    await _historyRepo.AddAsync(new BookingStatusHistory
    {
      BookingId = bookingId,
      OldStatus = oldStatus,
      NewStatus = newStatus,
      UpdatedBy = updatedBy
    });

    await _auditService.LogActionAsync(
      "BOOKING_STATUS_CHANGED", "Booking", updatedBy, 
      "USER", bookingId, null, 
      $"{{ \"old\": \"{oldStatus}\", \"new\": \"{newStatus}\" }}");
  }

  private BookingResponseDto MapToDto(Booking booking)
  {
    return new BookingResponseDto
    {
      Id = booking.Id,
      Status = booking.Status.ToString(),
      PaymentStatus = booking.PaymentStatus.ToString(),
      Address = booking.Address,
      Lat = booking.Lat,
      Lng = booking.Lng,
      Description = booking.Description,
      CreatedAt = booking.CreatedAt,
      Customer = new BookingPartyDto
      {
        Id = booking.CustomerId,
        FullName = booking.Customer?.FullName ?? "Unknown",
        Email = booking.Customer?.Email ?? string.Empty,
        AvatarUrl = booking.Customer?.AvatarUrl,
      },
      Worker = booking.Worker == null ? null : new BookingPartyDto
      {
        Id = booking.WorkerId!.Value,
        FullName = booking.Worker.FullName,
        Email = booking.Worker.Email,
        AvatarUrl = booking.Worker.AvatarUrl,
      },
      Service = new BookingServiceDto
      {
        Id = booking.Service?.Id ?? booking.ServiceId,
        Name = booking.Service?.Name ?? string.Empty,
      },
      Quotations = booking.Quotations.Select(q => new QuotationDto
      {
        Id = q.Id,
        BookingId = q.BookingId,
        WorkerId = q.WorkerId,
        CustomerId = q.CustomerId,
        Subtotal = q.Subtotal,
        TotalAmount = q.TotalAmount,
        Note = q.Note,
        Status = q.Status,
        CreatedAt = q.CreatedAt,
        ExpiresAt = q.ExpiresAt,
        Items = q.Items.Select(i => new QuotationItemDto
        {
          Id = i.Id,
          ItemName = i.ItemName,
          Quantity = i.Quantity,
          UnitPrice = i.UnitPrice,
          TotalPrice = i.TotalPrice
        }).ToList()
      }).ToList(),
      NavigationUrl = $"https://www.google.com/maps/dir/?api=1&destination={booking.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{booking.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
    };
  }
}
