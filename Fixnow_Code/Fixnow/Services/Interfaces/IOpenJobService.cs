using Fixnow.DTOs.OpenJob;

namespace Fixnow.Services.Interfaces;

public interface IOpenJobService
{
  Task<OpenJobResponse> CreateJobAsync(Guid customerId, CreateOpenJobRequest request);
  Task<IEnumerable<OpenJobResponse>> GetCustomerJobsAsync(Guid customerId);
  Task<IEnumerable<OpenJobResponse>> GetNearbyJobsAsync(Guid workerId, double lat, double lng);
  Task<IEnumerable<OpenJobResponse>> GetMarketplaceJobsAsync(
    Guid workerId,
    double lat,
    double lng,
    double radiusKm,
    List<Guid>? serviceIds,
    decimal? minBudget,
    decimal? maxBudget,
    string? urgencyLevel,
    string? sortBy);

  Task<OpenJobResponse> GetJobDetailsAsync(Guid jobId);
  Task<OfferResponse> SubmitOfferAsync(Guid workerId, Guid jobId, SubmitOfferRequest request);
  Task<IEnumerable<OfferResponse>> GetJobOffersAsync(Guid jobId);
  Task SelectWorkerAsync(Guid customerId, Guid jobId, Guid offerId);
  Task<OpenJobResponse> UpdateJobAsync(Guid customerId, Guid jobId, CreateOpenJobRequest request);
  Task CloseJobAsync(Guid customerId, Guid jobId, string? reason);
  Task RejectOfferAsync(Guid customerId, Guid offerId);

  // Admin
  Task<IEnumerable<OpenJobResponse>> GetAllJobsForAdminAsync();
  Task ModerateJobAsync(Guid adminId, Guid jobId, ModerationRequest request);
  Task DeleteJobAsync(Guid adminId, Guid jobId);

  // Saved Jobs
  Task SaveJobAsync(Guid workerId, Guid jobId);
  Task UnsaveJobAsync(Guid workerId, Guid jobId);
  Task<IEnumerable<OpenJobResponse>> GetSavedJobsAsync(Guid workerId);

  // Maintenance
  Task ProcessExpiredJobsAsync();
}
