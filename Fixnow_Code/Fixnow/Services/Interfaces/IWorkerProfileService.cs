using Fixnow.DTOs.WorkerProfile;
using Fixnow.Enums;

namespace Fixnow.Services.Interfaces;

public interface IWorkerProfileService
{
  Task<WorkerProfileDto> GetProfileAsync(Guid workerId);
  Task<WorkerProfileDto> UpdateProfileAsync(Guid workerId, UpdateWorkerProfileDto request);
  Task<WorkerProfileDto> UpdateAvailabilityAsync(Guid workerId, WorkerAvailability status);
  Task<WorkerProfileDto> UpdateSkillsAsync(Guid workerId, List<Guid> serviceIds);
}
