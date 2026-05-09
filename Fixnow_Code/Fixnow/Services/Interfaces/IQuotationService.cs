using Fixnow.DTOs.Quotation;

namespace Fixnow.Services.Interfaces;

public interface IQuotationService
{
  Task<QuotationDto> CreateQuotationAsync(CreateQuotationRequestDto request, Guid workerId);
  Task<QuotationDto> GetQuotationAsync(Guid quotationId);
  Task<List<QuotationDto>> GetQuotationsByBookingAsync(Guid bookingId);
  Task<QuotationDto> ApproveQuotationAsync(Guid quotationId, Guid customerId);
  Task<QuotationDto> RejectQuotationAsync(Guid quotationId, Guid customerId);
  Task ExpireQuotationAsync(Guid quotationId);
}
