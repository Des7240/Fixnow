using Fixnow.DTOs.Admin;
using Fixnow.DTOs.Auth;
using Fixnow.DTOs.Kyc;

namespace Fixnow.Services.Interfaces;

public interface IAdminService
{
  Task<List<KycResponseDto>> GetAllKycsAsync();
  Task<List<UserDto>> GetAllUsersAsync();
  Task<List<UserDto>> GetAllWorkersAsync();
  Task<KycResponseDto> ReviewKycAsync(Guid kycId, Guid adminId, ReviewKycDto request);
  Task SuspendWorkerAsync(Guid workerId, Guid adminId);
  Task ActivateWorkerAsync(Guid workerId, Guid adminId);
  Task UpdateUserStatusAsync(Guid userId, string status, Guid adminId);
  Task<DashboardSummaryDto> GetDashboardSummaryAsync();
  Task<List<PendingWorkerServiceDto>> GetPendingWorkerServicesAsync();
  Task ReviewWorkerServiceAsync(Guid workerId, Guid serviceId, Guid adminId, ReviewWorkerServiceDto request);

  // Phase 6: Dynamic Config & Service CRUD
  Task UpdateSystemConfigAsync(string key, string value, Guid adminId);
  Task<List<Entities.SystemConfig>> GetAllConfigsAsync();
  Task UpdateServiceCommissionAsync(Guid serviceId, decimal percent, Guid adminId);
  Task<List<Entities.ServiceCommission>> GetAllCommissionsAsync();
  
  // Service CRUD
  Task<Entities.ServiceCategory> CreateServiceAsync(DTOs.Admin.CreateServiceRequestDto request, Guid adminId);
  Task<Entities.ServiceCategory> UpdateServiceAsync(Guid serviceId, DTOs.Admin.UpdateServiceRequestDto request, Guid adminId);
  Task DeleteServiceAsync(Guid serviceId, Guid adminId);
}
