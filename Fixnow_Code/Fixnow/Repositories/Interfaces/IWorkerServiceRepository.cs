using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IWorkerServiceRepository
{
  Task<List<WorkerService>> FindByWorkerIdAsync(Guid workerId);
  Task UpdateWorkerServicesAsync(Guid workerId, List<Guid> serviceIds);
  Task<List<WorkerService>> GetPendingServicesAsync();
  Task UpdateServiceStatusAsync(Guid workerId, Guid serviceId, Enums.WorkerServiceStatus status);
}
