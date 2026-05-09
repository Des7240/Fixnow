using Fixnow.Entities;
using Fixnow.Enums;

namespace Fixnow.Repositories.Interfaces;

public interface IQuotationRepository
{
  Task<Quotation> CreateAsync(Quotation quotation);
  Task<Quotation?> FindByIdAsync(Guid id);
  Task<Quotation?> FindByIdWithDetailsAsync(Guid id);
  Task<List<Quotation>> FindByBookingIdAsync(Guid bookingId);
  Task UpdateAsync(Quotation quotation);
}
