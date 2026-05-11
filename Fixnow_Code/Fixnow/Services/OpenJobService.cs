using Fixnow.Data;
using Fixnow.DTOs.OpenJob;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Fixnow.Services;

public class OpenJobService : IOpenJobService
{
  private readonly IOpenJobRepository _openJobRepo;
  private readonly IOfferRepository _offerRepo;
  private readonly IWorkerLocationRepository _workerLocationRepo;
  private readonly IUserRepository _userRepo;
  private readonly INotificationService _notificationService;
  private readonly IBookingRepository _bookingRepo;
  private readonly IConversationRepository _conversationRepo;
  private readonly IBookingStatusHistoryRepository _historyRepo;
  private readonly IAuditService _auditService;
  private readonly AppDbContext _context;
  private static readonly GeometryFactory GeomFactory = new(new PrecisionModel(), 4326);

  public OpenJobService(
    IOpenJobRepository openJobRepo,
    IOfferRepository offerRepo,
    IWorkerLocationRepository workerLocationRepo,
    IUserRepository userRepo,
    INotificationService notificationService,
    IBookingRepository bookingRepo,
    IConversationRepository conversationRepo,
    IBookingStatusHistoryRepository historyRepo,
    IAuditService auditService,
    AppDbContext context)
  {
    _openJobRepo = openJobRepo;
    _offerRepo = offerRepo;
    _workerLocationRepo = workerLocationRepo;
    _userRepo = userRepo;
    _notificationService = notificationService;
    _bookingRepo = bookingRepo;
    _conversationRepo = conversationRepo;
    _historyRepo = historyRepo;
    _auditService = auditService;
    _context = context;
  }

  public async Task<OpenJobResponse> CreateJobAsync(Guid customerId, CreateOpenJobRequest request)
  {
    var location = GeomFactory.CreatePoint(new Coordinate(request.Lng, request.Lat));

    var openJob = new OpenJob
    {
      CustomerId = customerId,
      ServiceId = request.ServiceId,
      Title = request.Title,
      Description = request.Description,
      Address = request.Address,
      Lat = request.Lat,
      Lng = request.Lng,
      Location = location,
      RadiusKm = request.RadiusKm,
      Status = OpenJobStatus.OPEN,
      CreatedAt = DateTime.UtcNow
    };

    foreach (var fileId in request.FileIds)
    {
      openJob.Attachments.Add(new OpenJobAttachment { FileId = fileId });
    }

    await _openJobRepo.AddAsync(openJob);

    // Find nearby workers and notify them
    var nearbyWorkers = await _workerLocationRepo.FindNearbyAvailableWorkersAsync(
      request.Lat, request.Lng, request.ServiceId, request.RadiusKm * 1000);

    foreach (var worker in nearbyWorkers)
    {
      await _notificationService.NotifyWorkerNewOpenJobAsync(worker.WorkerId, openJob.Id, openJob.Title);
    }

    var result = await _openJobRepo.GetByIdAsync(openJob.Id);
    return MapToOpenJobResponse(result!);
  }

  public async Task<IEnumerable<OpenJobResponse>> GetCustomerJobsAsync(Guid customerId)
  {
    var jobs = await _openJobRepo.GetByCustomerIdAsync(customerId);
    return jobs.Select(MapToOpenJobResponse);
  }

  public async Task<IEnumerable<OpenJobResponse>> GetNearbyJobsAsync(Guid workerId, double lat, double lng)
  {
    var skills = await _context.WorkerServices
      .Where(ws => ws.WorkerId == workerId && ws.Status == WorkerServiceStatus.APPROVED)
      .Select(ws => ws.ServiceId)
      .ToListAsync();

    if (!skills.Any()) return Enumerable.Empty<OpenJobResponse>();

    var jobs = await _openJobRepo.GetNearbyJobsAsync(lat, lng, skills);
    return jobs.Select(MapToOpenJobResponse);
  }

  public async Task<OpenJobResponse> GetJobDetailsAsync(Guid jobId)
  {
    var job = await _openJobRepo.GetByIdAsync(jobId)
      ?? throw new KeyNotFoundException("Job not found.");

    var response = MapToOpenJobResponse(job);
    response.OfferCount = await _context.WorkerOffers.CountAsync(o => o.OpenJobId == jobId);

    return response;
  }

  public async Task<OfferResponse> SubmitOfferAsync(Guid workerId, Guid jobId, SubmitOfferRequest request)
  {
    var job = await _openJobRepo.GetByIdAsync(jobId)
      ?? throw new KeyNotFoundException("Job not found.");

    if (job.Status != OpenJobStatus.OPEN && job.Status != OpenJobStatus.RECEIVING_OFFERS)
      throw new InvalidOperationException("This job is no longer accepting offers.");

    var existingOffer = await _context.WorkerOffers
      .FirstOrDefaultAsync(o => o.OpenJobId == jobId && o.WorkerId == workerId);

    if (existingOffer != null)
      throw new InvalidOperationException("You have already submitted an offer for this job.");

    var offer = new WorkerOffer
    {
      OpenJobId = jobId,
      WorkerId = workerId,
      EstimatedPrice = request.EstimatedPrice,
      Analysis = request.Analysis,
      EstimatedArrivalMinutes = request.EstimatedArrivalMinutes,
      Status = OfferStatus.SUBMITTED,
      CreatedAt = DateTime.UtcNow
    };

    foreach (var fileId in request.FileIds)
    {
      offer.Attachments.Add(new OfferAttachment { FileId = fileId });
    }

    await _offerRepo.AddAsync(offer);

    if (job.Status == OpenJobStatus.OPEN)
    {
      job.Status = OpenJobStatus.RECEIVING_OFFERS;
      await _openJobRepo.UpdateAsync(job);
    }

    var worker = await _userRepo.FindByIdAsync(workerId);
    await _notificationService.NotifyCustomerNewOfferAsync(job.CustomerId, job.Id, worker?.FullName ?? "Thợ");

    var result = await _offerRepo.GetByIdAsync(offer.Id);
    return MapToOfferResponse(result!);
  }

  public async Task<IEnumerable<OfferResponse>> GetJobOffersAsync(Guid jobId)
  {
    var offers = await _offerRepo.GetByOpenJobIdAsync(jobId);
    var results = new List<OfferResponse>();

    foreach (var offer in offers)
    {
      var dto = MapToOfferResponse(offer);

      var ratingSummary = await _context.WorkerRatingSummaries.FindAsync(offer.WorkerId);
      dto.WorkerRating = ratingSummary?.AverageRating ?? 0;
      dto.WorkerCompletedJobs = ratingSummary?.TotalReviews ?? 0;

      results.Add(dto);
    }

    return results;
  }

  public async Task SelectWorkerAsync(Guid customerId, Guid jobId, Guid offerId)
  {
    var job = await _openJobRepo.GetByIdAsync(jobId)
      ?? throw new KeyNotFoundException("Job not found.");

    if (job.CustomerId != customerId)
      throw new UnauthorizedAccessException("Only the job owner can select a worker.");

    if (job.Status != OpenJobStatus.RECEIVING_OFFERS && job.Status != OpenJobStatus.OPEN)
      throw new InvalidOperationException("Job is not in a state to select a worker.");

    var offer = await _offerRepo.GetByIdAsync(offerId)
      ?? throw new KeyNotFoundException("Offer not found.");

    if (offer.OpenJobId != jobId)
      throw new InvalidOperationException("Offer does not belong to this job.");

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Update offer and job statuses
      offer.Status = OfferStatus.ACCEPTED;
      job.Status = OpenJobStatus.WORKER_SELECTED;
      await _context.SaveChangesAsync();

      // Reject other offers
      var otherOffers = await _context.WorkerOffers
        .Where(o => o.OpenJobId == jobId && o.Id != offerId)
        .ToListAsync();

      foreach (var other in otherOffers)
      {
        other.Status = OfferStatus.REJECTED;
      }
      await _context.SaveChangesAsync();

      // Create booking
      var booking = new Booking
      {
        CustomerId = customerId,
        WorkerId = offer.WorkerId,
        ServiceId = job.ServiceId,
        Address = job.Address,
        Lat = job.Lat,
        Lng = job.Lng,
        Location = job.Location,
        Description = job.Description,
        Status = BookingStatus.ASSIGNED,
        OpenJobId = job.Id,
        TotalAmount = offer.EstimatedPrice
      };

      await _bookingRepo.CreateAsync(booking);

      // Log status history for booking
      await _historyRepo.AddAsync(new BookingStatusHistory
      {
        BookingId = booking.Id,
        NewStatus = BookingStatus.ASSIGNED,
        UpdatedBy = customerId
      });

      // Create conversation
      await _conversationRepo.CreateAsync(new Conversation
      {
        BookingId = booking.Id,
        CustomerId = customerId,
        WorkerId = offer.WorkerId
      });

      job.Status = OpenJobStatus.BOOKING_CREATED;
      offer.Status = OfferStatus.BOOKING_CREATED;
      await _context.SaveChangesAsync();

      await transaction.CommitAsync();

      await _notificationService.NotifyWorkerOfferAcceptedAsync(offer.WorkerId, booking.Id);
      
      await _auditService.LogActionAsync(
        "OPEN_JOB_WORKER_SELECTED", "OpenJob", customerId, 
        "CUSTOMER", job.Id, null, 
        $"{{ \"workerId\": \"{offer.WorkerId}\", \"bookingId\": \"{booking.Id}\" }}");
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  private OpenJobResponse MapToOpenJobResponse(OpenJob job)
  {
    return new OpenJobResponse
    {
      Id = job.Id,
      CustomerId = job.CustomerId,
      CustomerName = job.Customer?.FullName ?? "Unknown",
      ServiceId = job.ServiceId,
      ServiceName = job.Service?.Name ?? "Unknown",
      Title = job.Title,
      Description = job.Description,
      Address = job.Address,
      Lat = job.Lat,
      Lng = job.Lng,
      RadiusKm = job.RadiusKm,
      Status = job.Status,
      CreatedAt = job.CreatedAt,
      FileUrls = job.Attachments.Select(a => a.File.ObjectKey).ToList()
    };
  }

  private OfferResponse MapToOfferResponse(WorkerOffer offer)
  {
    return new OfferResponse
    {
      Id = offer.Id,
      OpenJobId = offer.OpenJobId,
      WorkerId = offer.WorkerId,
      WorkerName = offer.Worker?.FullName ?? "Unknown",
      EstimatedPrice = offer.EstimatedPrice,
      Analysis = offer.Analysis,
      EstimatedArrivalMinutes = offer.EstimatedArrivalMinutes,
      Status = offer.Status,
      CreatedAt = offer.CreatedAt,
      FileUrls = offer.Attachments.Select(a => a.File.ObjectKey).ToList()
    };
  }
}
