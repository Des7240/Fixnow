using Fixnow.DTOs.WorkerProfile;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class WorkerProfileService : IWorkerProfileService
{
  private readonly IWorkerProfileRepository _profileRepo;
  private readonly IWorkerServiceRepository _workerServiceRepo;
  private readonly IUserRepository _userRepo;

  public WorkerProfileService(
    IWorkerProfileRepository profileRepo,
    IWorkerServiceRepository workerServiceRepo,
    IUserRepository userRepo)
  {
    _profileRepo = profileRepo;
    _workerServiceRepo = workerServiceRepo;
    _userRepo = userRepo;
  }

  public async Task<WorkerProfileDto> GetProfileAsync(Guid workerId)
  {
    var user = await _userRepo.FindByIdAsync(workerId)
      ?? throw new KeyNotFoundException("Worker not found.");

    var profile = await _profileRepo.FindByWorkerIdAsync(workerId);
    var services = await _workerServiceRepo.FindByWorkerIdAsync(workerId);

    return MapToDto(user, profile, services);
  }

  public async Task<WorkerProfileDto> UpdateProfileAsync(Guid workerId, UpdateWorkerProfileDto request)
  {
    var profile = await _profileRepo.FindByWorkerIdAsync(workerId);
    if (profile is null)
    {
      profile = new WorkerProfile
      {
        UserId = workerId,
        Bio = request.Bio,
        ExperienceYears = request.ExperienceYears
      };
      await _profileRepo.CreateAsync(profile);
    }
    else
    {
      profile.Bio = request.Bio;
      profile.ExperienceYears = request.ExperienceYears;
      await _profileRepo.UpdateAsync(profile);
    }

    return await GetProfileAsync(workerId);
  }

  public async Task<WorkerProfileDto> UpdateAvailabilityAsync(Guid workerId, WorkerAvailability status)
  {
    var profile = await _profileRepo.FindByWorkerIdAsync(workerId);
    if (profile is null)
    {
      profile = new WorkerProfile
      {
        UserId = workerId,
        AvailabilityStatus = status
      };
      await _profileRepo.CreateAsync(profile);
    }
    else
    {
      profile.AvailabilityStatus = status;
      await _profileRepo.UpdateAsync(profile);
    }

    return await GetProfileAsync(workerId);
  }

  public async Task<WorkerProfileDto> UpdateSkillsAsync(Guid workerId, List<Guid> serviceIds)
  {
    await _workerServiceRepo.UpdateWorkerServicesAsync(workerId, serviceIds);
    return await GetProfileAsync(workerId);
  }

  private static WorkerProfileDto MapToDto(User user, WorkerProfile? profile, List<WorkerService> services)
  {
    return new WorkerProfileDto
    {
      UserId = user.Id,
      FullName = user.FullName,
      Email = user.Email,
      Bio = profile?.Bio,
      ExperienceYears = profile?.ExperienceYears ?? 0,
      AverageRating = profile?.AverageRating ?? 0,
      TotalJobs = profile?.TotalJobs ?? 0,
      AvailabilityStatus = profile?.AvailabilityStatus ?? WorkerAvailability.OFFLINE,
      Skills = services.Select(s => new WorkerServiceDto
      {
        ServiceId = s.ServiceId,
        ServiceName = s.Service.Name
      }).ToList()
    };
  }
}
