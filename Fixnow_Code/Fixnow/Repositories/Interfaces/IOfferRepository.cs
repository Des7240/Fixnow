using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IOfferRepository
{
  Task<WorkerOffer?> GetByIdAsync(Guid id);
  Task<IEnumerable<WorkerOffer>> GetByOpenJobIdAsync(Guid openJobId);
  Task<IEnumerable<WorkerOffer>> GetByWorkerIdAsync(Guid workerId);
  Task AddAsync(WorkerOffer offer);
  Task UpdateAsync(WorkerOffer offer);
}
