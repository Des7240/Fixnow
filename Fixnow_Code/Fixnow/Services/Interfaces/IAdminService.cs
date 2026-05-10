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
}
