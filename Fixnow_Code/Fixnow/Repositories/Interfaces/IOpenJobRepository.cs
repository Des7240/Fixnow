using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IOpenJobRepository
{
  Task<OpenJob?> GetByIdAsync(Guid id);
  Task<IEnumerable<OpenJob>> GetByCustomerIdAsync(Guid customerId);
  Task<IEnumerable<OpenJob>> GetNearbyJobsAsync(double lat, double lng, List<Guid> serviceIds);
  Task AddAsync(OpenJob openJob);
  Task UpdateAsync(OpenJob openJob);
  Task DeleteAsync(OpenJob openJob);
}
