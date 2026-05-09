using Fixnow.DTOs.Payment;

namespace Fixnow.Services.Interfaces;

public interface IPaymentService
{
  Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid customerId, string ipAddress);
  Task<PaymentResultDto> ProcessCallbackAsync(string providerName, IQueryCollection query);
}
