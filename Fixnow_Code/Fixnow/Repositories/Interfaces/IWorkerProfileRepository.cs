using Fixnow.Entities;
using Fixnow.Enums;

namespace Fixnow.Repositories.Interfaces;

public interface IWorkerProfileRepository
{
  Task<WorkerProfile?> FindByWorkerIdAsync(Guid workerId);
  Task<WorkerProfile> CreateAsync(WorkerProfile profile);
  Task UpdateAsync(WorkerProfile profile);
}
