using Fixnow.DTOs.Quotation;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Hangfire;

namespace Fixnow.Services;

public class QuotationService : IQuotationService
{
  private readonly IQuotationRepository _quotationRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly INotificationService _notificationService;
  private readonly IBackgroundJobClient _backgroundJobClient;
  private readonly IAuditService _auditService;
  private readonly IBookingStatusHistoryRepository _historyRepo;
  private readonly INotificationRepository _notificationRepo;

  public QuotationService(
    IQuotationRepository quotationRepo,
    IBookingRepository bookingRepo,
    INotificationService notificationService,
    IBackgroundJobClient backgroundJobClient,
    IAuditService auditService,
    IBookingStatusHistoryRepository historyRepo,
    INotificationRepository notificationRepo)
  {
    _quotationRepo = quotationRepo;
    _bookingRepo = bookingRepo;
    _notificationService = notificationService;
    _backgroundJobClient = backgroundJobClient;
    _auditService = auditService;
    _historyRepo = historyRepo;
    _notificationRepo = notificationRepo;
  }

  public async Task<QuotationDto> CreateQuotationAsync(CreateQuotationRequestDto request, Guid workerId)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(request.BookingId)
      ?? throw new KeyNotFoundException("Booking not found");

    if (booking.WorkerId != workerId)
      throw new UnauthorizedAccessException("You are not the assigned worker for this booking.");

    if (booking.Status != BookingStatus.ASSIGNED && booking.Status != BookingStatus.INSPECTING && booking.Status != BookingStatus.QUOTED)
      throw new InvalidOperationException($"Cannot create quotation in current booking status: {booking.Status}");

    // If there is an existing PENDING quote, maybe reject it or throw error. We'll allow multiple for now or just expire old ones.
    var existingQuotes = await _quotationRepo.FindByBookingIdAsync(booking.Id);
    foreach (var q in existingQuotes.Where(x => x.Status == QuotationStatus.PENDING))
    {
      q.Status = QuotationStatus.EXPIRED;
      await _quotationRepo.UpdateAsync(q);
    }

    var quotation = new Quotation
    {
      BookingId = booking.Id,
      WorkerId = workerId,
      CustomerId = booking.CustomerId,
      Note = request.Note,
      Status = QuotationStatus.PENDING,
      ExpiresAt = DateTime.UtcNow.AddHours(24)
    };

    decimal totalAmount = 0;
    foreach (var item in request.Items)
    {
      var totalPrice = item.Quantity * item.UnitPrice;
      totalAmount += totalPrice;

      quotation.Items.Add(new QuotationItem
      {
        ItemName = item.ItemName,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        TotalPrice = totalPrice
      });
    }

    quotation.Subtotal = totalAmount;
    quotation.TotalAmount = totalAmount;

    await _quotationRepo.CreateAsync(quotation);

    // Update booking status
    var oldStatus = booking.Status;
    if (booking.Status != BookingStatus.QUOTED)
    {
      booking.Status = BookingStatus.QUOTED;
      await _bookingRepo.UpdateAsync(booking);
      
      await _historyRepo.AddAsync(new BookingStatusHistory
      {
        BookingId = booking.Id,
        OldStatus = oldStatus,
        NewStatus = BookingStatus.QUOTED,
        UpdatedBy = workerId
      });
    }

    // Schedule Expiration Job
    _backgroundJobClient.Schedule<IQuotationService>(
      x => x.ExpireQuotationAsync(quotation.Id),
      TimeSpan.FromHours(24)
    );

    // Notify Customer
    // To speed up, we just use a generic push using notification service
    _backgroundJobClient.Enqueue(() => 
      _notificationService.NotifyCustomerBookingStatusAsync(booking.CustomerId, booking.Id, "QUOTED"));

    await _auditService.LogActionAsync("QUOTE_CREATED", "Quotation", workerId, "WORKER", quotation.Id, null, $"Total: {totalAmount}");

    return MapToDto(quotation);
  }

  public async Task<QuotationDto> GetQuotationAsync(Guid quotationId)
  {
    var quotation = await _quotationRepo.FindByIdWithDetailsAsync(quotationId)
      ?? throw new KeyNotFoundException("Quotation not found.");
    return MapToDto(quotation);
  }

  public async Task<List<QuotationDto>> GetQuotationsByBookingAsync(Guid bookingId)
  {
    var quotations = await _quotationRepo.FindByBookingIdAsync(bookingId);
    return quotations.Select(MapToDto).ToList();
  }

  public async Task<QuotationDto> ApproveQuotationAsync(Guid quotationId, Guid customerId)
  {
    var quotation = await _quotationRepo.FindByIdWithDetailsAsync(quotationId)
      ?? throw new KeyNotFoundException("Quotation not found.");

    if (quotation.CustomerId != customerId)
      throw new UnauthorizedAccessException("You can only approve your own quotations.");

    if (quotation.Status != QuotationStatus.PENDING)
      throw new InvalidOperationException($"Cannot approve quote in status {quotation.Status}");

    // Update quote
    quotation.Status = QuotationStatus.APPROVED;
    await _quotationRepo.UpdateAsync(quotation);

    // Update booking
    var booking = quotation.Booking;
    var oldStatus = booking.Status;

    if (booking.Status != BookingStatus.QUOTED && booking.Status != BookingStatus.WORKING)
      throw new InvalidOperationException($"Cannot approve quotation for booking in status {booking.Status}");

    booking.TotalAmount = quotation.TotalAmount;
    if (booking.Status != BookingStatus.WORKING)
    {
      booking.Status = BookingStatus.WORKING;
      await _bookingRepo.UpdateAsync(booking);

      await _historyRepo.AddAsync(new BookingStatusHistory
      {
        BookingId = booking.Id,
        OldStatus = oldStatus,
        NewStatus = BookingStatus.WORKING,
        UpdatedBy = customerId
      });
    }
    else
    {
      // Just update TotalAmount if already WORKING
      await _bookingRepo.UpdateAsync(booking);
    }

    // Notify Worker
    await _notificationRepo.AddAsync(new Notification
    {
      UserId = quotation.WorkerId,
      Title = "Báo giá được chấp thuận",
      Content = $"Khách hàng đã duyệt báo giá {quotation.TotalAmount:N0} VNĐ. Vui lòng bắt đầu công việc.",
      Type = "QUOTE_APPROVED",
      ReferenceId = booking.Id
    });

    await _auditService.LogActionAsync("QUOTE_APPROVED", "Quotation", customerId, "CUSTOMER", quotation.Id, null, "Customer approved quotation");

    return MapToDto(quotation);
  }

  public async Task<QuotationDto> RejectQuotationAsync(Guid quotationId, Guid customerId)
  {
    var quotation = await _quotationRepo.FindByIdWithDetailsAsync(quotationId)
      ?? throw new KeyNotFoundException("Quotation not found.");

    if (quotation.CustomerId != customerId)
      throw new UnauthorizedAccessException("You can only reject your own quotations.");

    if (quotation.Status != QuotationStatus.PENDING)
      throw new InvalidOperationException($"Cannot reject quote in status {quotation.Status}");

    quotation.Status = QuotationStatus.REJECTED;
    await _quotationRepo.UpdateAsync(quotation);

    // Revert booking status? Or stay QUOTED/QUOTE_REJECTED.
    var booking = quotation.Booking;
    var oldStatus = booking.Status;
    booking.Status = BookingStatus.QUOTE_REJECTED;
    await _bookingRepo.UpdateAsync(booking);

    await _historyRepo.AddAsync(new BookingStatusHistory
    {
      BookingId = booking.Id,
      OldStatus = oldStatus,
      NewStatus = BookingStatus.QUOTE_REJECTED,
      UpdatedBy = customerId
    });

    // Notify Worker
    await _notificationRepo.AddAsync(new Notification
    {
      UserId = quotation.WorkerId,
      Title = "Báo giá bị từ chối",
      Content = $"Khách hàng đã từ chối báo giá. Vui lòng liên hệ lại hoặc tạo báo giá mới.",
      Type = "QUOTE_REJECTED",
      ReferenceId = booking.Id
    });

    await _auditService.LogActionAsync("QUOTE_REJECTED", "Quotation", customerId, "CUSTOMER", quotation.Id, null, "Customer rejected quotation");

    return MapToDto(quotation);
  }

  public async Task ExpireQuotationAsync(Guid quotationId)
  {
    var quotation = await _quotationRepo.FindByIdWithDetailsAsync(quotationId);
    if (quotation != null && quotation.Status == QuotationStatus.PENDING)
    {
      quotation.Status = QuotationStatus.EXPIRED;
      await _quotationRepo.UpdateAsync(quotation);

      // Notify Worker & Customer
      await _notificationRepo.AddAsync(new Notification
      {
        UserId = quotation.WorkerId,
        Title = "Báo giá hết hạn",
        Content = $"Báo giá cho đơn hàng đã hết hạn sau 24h.",
        Type = "QUOTE_EXPIRED",
        ReferenceId = quotation.BookingId
      });
    }
  }

  private static QuotationDto MapToDto(Quotation q)
  {
    return new QuotationDto
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
    };
  }
}
