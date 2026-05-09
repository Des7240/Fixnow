using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Kyc;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Fixnow.Data;
using Microsoft.EntityFrameworkCore;

namespace Fixnow.Services;

public class AdminService : IAdminService
{
  private readonly IWorkerKycRepository _kycRepo;
  private readonly IUserRepository _userRepo;
  private readonly AppDbContext _db;
  private readonly IAuditService _auditService;

  public AdminService(IWorkerKycRepository kycRepo, IUserRepository userRepo, AppDbContext db, IAuditService auditService)
  {
    _kycRepo = kycRepo;
    _userRepo = userRepo;
    _db = db;
    _auditService = auditService;
  }

  public async Task<List<KycResponseDto>> GetAllKycsAsync()
  {
    var kycs = await _kycRepo.GetAllAsync();
    return kycs.Select(k => new KycResponseDto
    {
      Id = k.Id,
      CitizenIdNumber = k.CitizenIdNumber,
      Status = k.Status.ToString(),
      RejectionReason = k.RejectionReason,
      SubmittedAt = k.SubmittedAt,
      VerifiedAt = k.VerifiedAt,
      WorkerName = k.Worker?.FullName,
      CitizenFrontUrl = k.CitizenFrontUrl,
      CitizenBackUrl = k.CitizenBackUrl,
      SelfieUrl = k.SelfieUrl
    }).ToList();
  }

  public async Task<List<UserDto>> GetAllWorkersAsync()
  {
    var workers = await _userRepo.GetByRoleAsync(UserRole.WORKER);
    return workers.Select(w => new UserDto
    {
      Id = w.Id,
      Email = w.Email,
      FullName = w.FullName,
      Role = w.Role.ToString(),
      Status = w.Status,
      CreatedAt = w.CreatedAt
    }).ToList();
  }

  public async Task<KycResponseDto> ReviewKycAsync(Guid kycId, Guid adminId, ReviewKycDto request)
  {
    var kyc = await _kycRepo.FindByIdAsync(kycId)
      ?? throw new KeyNotFoundException("KYC record not found.");

    kyc.Status = request.Status;
    kyc.RejectionReason = request.Reason;
    kyc.VerifiedBy = adminId;
    kyc.VerifiedAt = DateTime.UtcNow;

    await _kycRepo.UpdateAsync(kyc);

    var actionType = request.Status == KycStatus.APPROVED ? "KYC_APPROVED" : "KYC_REJECTED";
    await _auditService.LogActionAsync(actionType, "WorkerKyc", adminId, "ADMIN", kycId, null, $"{{ \"reason\": \"{request.Reason}\" }}");

    return new KycResponseDto
    {
      Id = kyc.Id,
      CitizenIdNumber = kyc.CitizenIdNumber,
      Status = kyc.Status.ToString(),
      RejectionReason = kyc.RejectionReason,
      SubmittedAt = kyc.SubmittedAt,
      VerifiedAt = kyc.VerifiedAt
    };
  }

  public async Task SuspendWorkerAsync(Guid workerId, Guid adminId)
  {
    var worker = await _userRepo.FindByIdAsync(workerId)
      ?? throw new KeyNotFoundException("Worker not found.");

    worker.Status = "BANNED";
    await _userRepo.UpdateAsync(worker);

    await _auditService.LogActionAsync("WORKER_SUSPENDED", "User", adminId, "ADMIN", workerId, null, null);
  }

  public async Task ActivateWorkerAsync(Guid workerId, Guid adminId)
  {
    var worker = await _userRepo.FindByIdAsync(workerId)
      ?? throw new KeyNotFoundException("Worker not found.");

    worker.Status = "ACTIVE";
    await _userRepo.UpdateAsync(worker);

    await _auditService.LogActionAsync("WORKER_ACTIVATED", "User", adminId, "ADMIN", workerId, null, null);
  }

  public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
  {
    var totalBookings = await _db.Bookings.CountAsync();
    var completedBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.COMPLETED);
    var cancelledBookings = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.CANCELLED);
    
    double cancelRate = totalBookings > 0 ? Math.Round((double)cancelledBookings / totalBookings * 100, 2) : 0;

    var totalWorkers = await _db.Users.CountAsync(u => u.Role == UserRole.WORKER);
    var onlineWorkers = await _db.WorkerProfiles.CountAsync(w => w.AvailabilityStatus == WorkerAvailability.ONLINE);
    var pendingKycs = await _db.WorkerKycs.CountAsync(k => k.Status == KycStatus.PENDING);

    var avgRating = await _db.WorkerRatingSummaries.AverageAsync(r => (double?)r.AverageRating) ?? 0;

    return new DashboardSummaryDto
    {
      TotalBookings = totalBookings,
      CompletedBookings = completedBookings,
      CancelRate = cancelRate,
      TotalWorkers = totalWorkers,
      OnlineWorkers = onlineWorkers,
      PendingKycs = pendingKycs,
      AverageSystemRating = Math.Round(avgRating, 2)
    };
  }
}
