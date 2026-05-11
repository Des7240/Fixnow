using Fixnow.Entities;

namespace Fixnow.Repositories.Interfaces;

public interface IOpenJobRepository
{
  Task<OpenJob?> GetByIdAsync(Guid id);
  Task<IEnumerable<OpenJob>> GetByCustomerIdAsync(Guid customerId);
  Task<IEnumerable<OpenJob>> GetNearbyJobsAsync(double lat, double lng, List<Guid> serviceIds);
  Task<IEnumerable<OpenJob>> GetMarketplaceJobsAsync(
    double lat, 
    double lng, 
    double radiusKm, 
    List<Guid>? serviceIds, 
    decimal? minBudget, 
    decimal? maxBudget, 
    string? urgencyLevel, 
    string? sortBy);
  
  Task AddAsync(OpenJob openJob);
  Task UpdateAsync(OpenJob openJob);
  Task DeleteAsync(OpenJob openJob);
  Task<IEnumerable<OpenJob>> GetAllForAdminAsync();

  // Saved Jobs
  Task SaveJobAsync(SavedOpenJob savedJob);
  Task UnsaveJobAsync(Guid workerId, Guid jobId);
  Task<IEnumerable<OpenJob>> GetSavedJobsAsync(Guid workerId);
  Task<bool> IsJobSavedAsync(Guid workerId, Guid jobId);
}
