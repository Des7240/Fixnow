using Fixnow.DTOs.Payment;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services.Providers;

public class MoMoProvider : IPaymentProvider
{
  public string ProviderName => "MOMO";

  public Task<string> CreatePaymentUrlAsync(PaymentRequestDto request)
  {
    // Mock MoMo payment URL for MVP
    // In production, this would call MoMo API to get payUrl
    var redirectUrl = $"{request.ReturnUrl}?paymentId={request.PaymentId}&status=success&transId=MOMO{DateTime.UtcNow.Ticks}";
    var payUrl = $"https://test-payment.momo.vn/v2/gateway/pay?redirect={Uri.EscapeDataString(redirectUrl)}";
    
    return Task.FromResult(payUrl);
  }

  public Task<PaymentResultDto> VerifyCallbackAsync(IQueryCollection query)
  {
    // Mock verify
    var status = query["status"].ToString();
    var transId = query["transId"].ToString();
    
    var result = new PaymentResultDto
    {
      IsSuccess = status == "success",
      TransactionId = transId,
      RawResponse = System.Text.Json.JsonSerializer.Serialize(query.ToDictionary(k => k.Key, k => k.Value.ToString())),
      ErrorMessage = status == "success" ? null : "MoMo Payment Failed",
      Amount = 0 // Normally we'd get amount from callback
    };

    return Task.FromResult(result);
  }
}
