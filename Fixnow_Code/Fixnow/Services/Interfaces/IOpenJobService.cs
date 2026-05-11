using Fixnow.DTOs.OpenJob;

namespace Fixnow.Services.Interfaces;

public interface IOpenJobService
{
  Task<OpenJobResponse> CreateJobAsync(Guid customerId, CreateOpenJobRequest request);
  Task<IEnumerable<OpenJobResponse>> GetCustomerJobsAsync(Guid customerId);
  Task<IEnumerable<OpenJobResponse>> GetNearbyJobsAsync(Guid workerId, double lat, double lng);
  Task<OpenJobResponse> GetJobDetailsAsync(Guid jobId);
  Task<OfferResponse> SubmitOfferAsync(Guid workerId, Guid jobId, SubmitOfferRequest request);
  Task<IEnumerable<OfferResponse>> GetJobOffersAsync(Guid jobId);
  Task SelectWorkerAsync(Guid customerId, Guid jobId, Guid offerId);
}
