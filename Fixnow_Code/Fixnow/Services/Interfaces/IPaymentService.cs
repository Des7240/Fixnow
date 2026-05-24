using Fixnow.DTOs.Payment;
using Fixnow.DTOs.Wallet;

namespace Fixnow.Services.Interfaces;

public interface IPaymentService
{
  Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid customerId, string ipAddress);
  Task<CreatePaymentResponseDto> CreateWalletDepositAsync(CreateWalletDepositRequestDto request, Guid userId, string ipAddress);
  Task<PaymentResultDto> ProcessCallbackAsync(string providerName, IQueryCollection query);
  Task<PaymentResultDto> ProcessSePayWebhookAsync(SePayWebhookDto payload);
}
