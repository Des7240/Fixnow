using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IWorkerServiceRepository
{
  Task<List<WorkerService>> FindByWorkerIdAsync(Guid workerId);
  Task UpdateWorkerServicesAsync(Guid workerId, List<Guid> serviceIds);
}
