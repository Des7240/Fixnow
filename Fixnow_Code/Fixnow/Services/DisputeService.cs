using Fixnow.Data;
using Fixnow.DTOs.Dispute;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class DisputeService : IDisputeService
{
  private readonly IDisputeRepository _disputeRepo;
  private readonly IRefundRepository _refundRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly IWalletRepository _walletRepo;
  private readonly IWalletTransactionRepository _walletTxRepo;
  private readonly IFileService _fileService;
  private readonly IAuditService _auditService;
  private readonly AppDbContext _db;

  public DisputeService(
    IDisputeRepository disputeRepo,
    IRefundRepository refundRepo,
    IBookingRepository bookingRepo,
    IWalletRepository walletRepo,
    IWalletTransactionRepository walletTxRepo,
    IFileService fileService,
    IAuditService auditService,
    AppDbContext db)
  {
    _disputeRepo = disputeRepo;
    _refundRepo = refundRepo;
    _bookingRepo = bookingRepo;
    _walletRepo = walletRepo;
    _walletTxRepo = walletTxRepo;
    _fileService = fileService;
    _auditService = auditService;
    _db = db;
  }

  public async Task<DisputeDto> CreateDisputeAsync(Guid customerId, CreateDisputeDto request)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(request.BookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.CustomerId != customerId)
      throw new UnauthorizedAccessException("You are not the owner of this booking.");

    if (booking.WorkerId == null)
      throw new InvalidOperationException("Cannot dispute a booking without a worker.");

    // Only allow dispute if paid or completed
    if (booking.Status != BookingStatus.COMPLETED && booking.PaymentStatus != BookingPaymentStatus.PAID)
      throw new InvalidOperationException("Can only dispute completed or paid bookings.");

    var dispute = new Dispute
    {
      BookingId = booking.Id,
      CustomerId = customerId,
      WorkerId = booking.WorkerId.Value,
      Reason = request.Reason,
      Status = DisputeStatus.OPEN
    };

    await _disputeRepo.CreateAsync(dispute);
    await _auditService.LogActionAsync("DISPUTE_CREATED", "Dispute", customerId, "CUSTOMER", dispute.Id, null, request.Reason);

    return await GetDisputeAsync(dispute.Id);
  }

  public async Task<DisputeEvidenceDto> AddEvidenceAsync(Guid disputeId, Guid uploaderId, IFormFile file)
  {
    var dispute = await _disputeRepo.FindByIdAsync(disputeId)
      ?? throw new KeyNotFoundException("Dispute not found.");

    if (dispute.CustomerId != uploaderId && dispute.WorkerId != uploaderId)
      throw new UnauthorizedAccessException("You are not part of this dispute.");

    if (dispute.Status == DisputeStatus.CLOSED || dispute.Status == DisputeStatus.RESOLVED)
      throw new InvalidOperationException("Cannot add evidence to a closed dispute.");

    var uploadedFile = await _fileService.UploadFileAsync(file, uploaderId, "disputes");
    
    var evidence = new DisputeEvidence
    {
      DisputeId = disputeId,
      FileUrl = $"/api/v1/files/{uploadedFile.Id}", // Assume endpoint to get file
      UploadedBy = uploaderId
    };

    await _disputeRepo.AddEvidenceAsync(evidence);
    
    // Change state to INVESTIGATING if it was OPEN
    if (dispute.Status == DisputeStatus.OPEN)
    {
      dispute.Status = DisputeStatus.INVESTIGATING;
      await _disputeRepo.UpdateAsync(dispute);
    }

    await _auditService.LogActionAsync("EVIDENCE_UPLOADED", "DisputeEvidence", uploaderId, "USER", evidence.Id, null, evidence.FileUrl);

    return new DisputeEvidenceDto
    {
      Id = evidence.Id,
      FileUrl = evidence.FileUrl,
      UploadedBy = evidence.UploadedBy,
      CreatedAt = evidence.CreatedAt
    };
  }

  public async Task<DisputeDto> GetDisputeAsync(Guid disputeId)
  {
    var dispute = await _disputeRepo.FindByIdWithDetailsAsync(disputeId)
      ?? throw new KeyNotFoundException("Dispute not found.");
      
    return MapToDto(dispute);
  }

  public async Task<List<DisputeDto>> GetMyDisputesAsync(Guid userId)
  {
    var disputes = await _disputeRepo.FindByUserIdAsync(userId);
    return disputes.Select(MapToDto).ToList();
  }

  public async Task<List<DisputeDto>> GetAllDisputesAsync()
  {
    var disputes = await _disputeRepo.GetAllAsync();
    return disputes.Select(MapToDto).ToList();
  }

  public async Task<DisputeDto> ProcessRefundAsync(Guid adminId, Guid disputeId, RefundRequestDto request)
  {
    if (request.Amount <= 0)
      throw new ArgumentException("Refund amount must be positive.");

    var dispute = await _disputeRepo.FindByIdWithDetailsAsync(disputeId)
      ?? throw new KeyNotFoundException("Dispute not found.");

    if (dispute.Status == DisputeStatus.CLOSED || dispute.Status == DisputeStatus.RESOLVED)
      throw new InvalidOperationException("Cannot refund a closed dispute.");

    // Retrieve worker's wallet
    var workerWallet = await _walletRepo.FindByUserIdAsync(dispute.WorkerId);
    if (workerWallet == null)
      throw new InvalidOperationException("Worker wallet not found.");

    using var transaction = await _db.Database.BeginTransactionAsync();
    try
    {
      // 1. Deduct money from Worker's wallet (Ledger Adjustment)
      var balanceBefore = workerWallet.Balance;
      var balanceAfter = workerWallet.Balance - request.Amount;
      workerWallet.Balance = balanceAfter;
      
      await _walletTxRepo.CreateAsync(new WalletTransaction
      {
        WalletId = workerWallet.Id,
        Type = TransactionType.REFUND, // Trừ tiền từ ví thợ cho refund
        Amount = -request.Amount,
        BalanceBefore = balanceBefore,
        BalanceAfter = balanceAfter,
        ReferenceId = dispute.Id,
        Description = $"Refund deduction for dispute {dispute.Id}"
      });
      await _walletRepo.UpdateAsync(workerWallet);

      // 2. Create Refund Record (Status PENDING for Accountant to manually transfer back to Customer)
      var refund = new Refund
      {
        DisputeId = dispute.Id,
        Amount = request.Amount,
        RefundType = request.RefundType,
        Status = RefundStatus.PENDING,
        ProcessedBy = adminId
      };
      await _refundRepo.CreateAsync(refund);

      // 3. Update Dispute Status
      dispute.Status = DisputeStatus.REFUNDED;
      await _disputeRepo.UpdateAsync(dispute);

      await transaction.CommitAsync();

      await _auditService.LogActionAsync("REFUND_PROCESSED", "Refund", adminId, "ADMIN", refund.Id, null, $"Refund {request.Amount} for Dispute {dispute.Id}");

      return await GetDisputeAsync(dispute.Id);
    }
    catch
    {
      await transaction.RollbackAsync();
      throw;
    }
  }

  public async Task<DisputeDto> CloseDisputeAsync(Guid adminId, Guid disputeId)
  {
    var dispute = await _disputeRepo.FindByIdAsync(disputeId)
      ?? throw new KeyNotFoundException("Dispute not found.");

    dispute.Status = DisputeStatus.CLOSED;
    await _disputeRepo.UpdateAsync(dispute);
    
    await _auditService.LogActionAsync("DISPUTE_CLOSED", "Dispute", adminId, "ADMIN", dispute.Id, null, "Closed by Admin");
    
    return MapToDto(dispute);
  }

  private DisputeDto MapToDto(Dispute dispute)
  {
    return new DisputeDto
    {
      Id = dispute.Id,
      BookingId = dispute.BookingId,
      CustomerId = dispute.CustomerId,
      CustomerName = dispute.Customer?.FullName ?? "Unknown",
      WorkerId = dispute.WorkerId,
      WorkerName = dispute.Worker?.FullName ?? "Unknown",
      Reason = dispute.Reason,
      Status = dispute.Status.ToString(),
      CreatedAt = dispute.CreatedAt,
      Evidences = dispute.Evidences?.Select(e => new DisputeEvidenceDto
      {
        Id = e.Id,
        FileUrl = e.FileUrl,
        UploadedBy = e.UploadedBy,
        CreatedAt = e.CreatedAt
      }).ToList() ?? new List<DisputeEvidenceDto>(),
      Refunds = dispute.Refunds?.Select(r => new RefundDto
      {
        Id = r.Id,
        Amount = r.Amount,
        RefundType = r.RefundType.ToString(),
        Status = r.Status.ToString(),
        CreatedAt = r.CreatedAt
      }).ToList() ?? new List<RefundDto>()
    };
  }
}
