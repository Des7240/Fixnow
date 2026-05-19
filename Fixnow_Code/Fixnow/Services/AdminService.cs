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
  private readonly IWorkerServiceRepository _workerServiceRepo;
  private readonly AppDbContext _db;
  private readonly IAuditService _auditService;
  private readonly INotificationService _notificationService;

  public AdminService(
    IWorkerKycRepository kycRepo, 
    IUserRepository userRepo, 
    IWorkerServiceRepository workerServiceRepo,
    AppDbContext db, 
    IAuditService auditService,
    INotificationService notificationService)
  {
    _kycRepo = kycRepo;
    _userRepo = userRepo;
    _workerServiceRepo = workerServiceRepo;
    _db = db;
    _auditService = auditService;
    _notificationService = notificationService;
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
      SelfieUrl = k.SelfieUrl,
      CertificateUrl = k.CertificateUrl
    }).ToList();
  }

  public async Task<List<UserDto>> GetAllUsersAsync()
  {
    var users = await _userRepo.GetAllAsync();
    return users.Select(u => new UserDto
    {
      Id = u.Id,
      Email = u.Email,
      FullName = u.FullName,
      AvatarUrl = u.AvatarUrl,
      Role = u.Role.ToString(),
      Status = u.Status,
      CreatedAt = u.CreatedAt
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
      AvatarUrl = w.AvatarUrl,
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

    // Send notification to worker
    await _notificationService.NotifyWorkerKycStatusAsync(kyc.WorkerId, request.Status.ToString(), request.Reason);

    return new KycResponseDto
    {
      Id = kyc.Id,
      CitizenIdNumber = kyc.CitizenIdNumber,
      Status = kyc.Status.ToString(),
      RejectionReason = kyc.RejectionReason,
      SubmittedAt = kyc.SubmittedAt,
      VerifiedAt = kyc.VerifiedAt,
      CertificateUrl = kyc.CertificateUrl
    };
  }

  public async Task UpdateUserStatusAsync(Guid userId, string status, Guid adminId)
  {
    var user = await _userRepo.FindByIdAsync(userId)
      ?? throw new KeyNotFoundException("User not found.");

    user.Status = status;
    await _userRepo.UpdateAsync(user);

    await _auditService.LogActionAsync("USER_STATUS_UPDATED", "User", adminId, "ADMIN", userId, null, $"{{ \"newStatus\": \"{status}\" }}");
  }

  public async Task SuspendWorkerAsync(Guid workerId, Guid adminId)
  {
    await UpdateUserStatusAsync(workerId, "BANNED", adminId);
  }

  public async Task ActivateWorkerAsync(Guid workerId, Guid adminId)
  {
    await UpdateUserStatusAsync(workerId, "ACTIVE", adminId);
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

  public async Task<List<PendingWorkerServiceDto>> GetPendingWorkerServicesAsync()
  {
    var pending = await _workerServiceRepo.GetPendingServicesAsync();
    return pending.Select(ws => new PendingWorkerServiceDto
    {
      WorkerId = ws.WorkerId,
      WorkerName = ws.Worker.FullName,
      WorkerEmail = ws.Worker.Email,
      ServiceId = ws.ServiceId,
      ServiceName = ws.Service.Name,
      Status = ws.Status
    }).ToList();
  }

  public async Task ReviewWorkerServiceAsync(Guid workerId, Guid serviceId, Guid adminId, ReviewWorkerServiceDto request)
  {
    await _workerServiceRepo.UpdateServiceStatusAsync(workerId, serviceId, request.Status);
    await _auditService.LogActionAsync($"WORKER_SERVICE_{request.Status}", "WorkerService", adminId, "ADMIN", workerId, null, $"{{ \"serviceId\": \"{serviceId}\" }}");
  }

  // Phase 6: Dynamic Config & Service CRUD
  public async Task UpdateSystemConfigAsync(string key, string value, Guid adminId)
  {
    var config = await _db.SystemConfigs.FindAsync(key)
      ?? throw new KeyNotFoundException($"Config key {key} not found.");

    config.ConfigValue = value;
    config.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    await _auditService.LogActionAsync("SYSTEM_CONFIG_UPDATED", "SystemConfig", adminId, "ADMIN", null, null, $"{{ \"key\": \"{key}\", \"newValue\": \"{value}\" }}");
  }

  public async Task<List<Entities.SystemConfig>> GetAllConfigsAsync()
  {
    return await _db.SystemConfigs.ToListAsync();
  }

  public async Task UpdateServiceCommissionAsync(Guid serviceId, decimal percent, Guid adminId)
  {
    var commission = await _db.ServiceCommissions.FirstOrDefaultAsync(c => c.ServiceId == serviceId);
    if (commission == null)
    {
        commission = new Entities.ServiceCommission
        {
            ServiceId = serviceId,
            CommissionPercent = percent
        };
        _db.ServiceCommissions.Add(commission);
    }
    else
    {
        commission.CommissionPercent = percent;
        commission.UpdatedAt = DateTime.UtcNow;
    }
    await _db.SaveChangesAsync();

    await _auditService.LogActionAsync("SERVICE_COMMISSION_UPDATED", "ServiceCommission", adminId, "ADMIN", serviceId, null, $"{{ \"percent\": {percent} }}");
  }

  public async Task<List<Entities.ServiceCommission>> GetAllCommissionsAsync()
  {
    return await _db.ServiceCommissions.Include(c => c.Service).ToListAsync();
  }

  public async Task<Entities.ServiceCategory> CreateServiceAsync(CreateServiceRequestDto request, Guid adminId)
  {
    var service = new Entities.ServiceCategory
    {
        Name = request.Name,
        Description = request.Description,
        IconUrl = request.IconUrl,
        BasePrice = request.BasePrice,
        EstimatedDurationMinutes = request.EstimatedDurationMinutes,
        IsActive = true
    };

    _db.ServiceCategories.Add(service);
    await _db.SaveChangesAsync();

    await _auditService.LogActionAsync("SERVICE_CREATED", "ServiceCategory", adminId, "ADMIN", service.Id, null, $"{{ \"name\": \"{service.Name}\" }}");
    return service;
  }

  public async Task<Entities.ServiceCategory> UpdateServiceAsync(Guid serviceId, UpdateServiceRequestDto request, Guid adminId)
  {
    var service = await _db.ServiceCategories.FindAsync(serviceId)
      ?? throw new KeyNotFoundException("Service not found.");

    service.Name = request.Name;
    service.Description = request.Description;
    service.IconUrl = request.IconUrl;
    service.BasePrice = request.BasePrice;
    service.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
    service.IsActive = request.IsActive;

    await _db.SaveChangesAsync();

    await _auditService.LogActionAsync("SERVICE_UPDATED", "ServiceCategory", adminId, "ADMIN", service.Id, null, $"{{ \"name\": \"{service.Name}\" }}");
    return service;
  }

  public async Task DeleteServiceAsync(Guid serviceId, Guid adminId)
  {
    var service = await _db.ServiceCategories.FindAsync(serviceId)
      ?? throw new KeyNotFoundException("Service not found.");

    service.IsActive = false; // Soft delete
    await _db.SaveChangesAsync();

    await _auditService.LogActionAsync("SERVICE_DEACTIVATED", "ServiceCategory", adminId, "ADMIN", serviceId, null, null);
  }

  public async Task<List<Entities.ServiceCategory>> GetAllServicesAsync()
  {
    return await _db.ServiceCategories.ToListAsync();
  }
}
