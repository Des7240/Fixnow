using Fixnow.DTOs.Payment;

namespace Fixnow.Services.Interfaces;

public interface IPaymentProvider
{
  string ProviderName { get; }
  Task<string> CreatePaymentUrlAsync(PaymentRequestDto request);
  Task<PaymentResultDto> VerifyCallbackAsync(IQueryCollection query);
}
