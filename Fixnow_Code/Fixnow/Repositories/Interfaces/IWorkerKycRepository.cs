using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IWorkerKycRepository
{
  Task<WorkerKyc?> FindByIdAsync(Guid id);
  Task<WorkerKyc?> FindLatestByWorkerIdAsync(Guid workerId);
  Task<List<WorkerKyc>> GetAllAsync();
  Task<WorkerKyc> CreateAsync(WorkerKyc kyc);
  Task UpdateAsync(WorkerKyc kyc);
}
