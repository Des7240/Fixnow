using Fixnow.DTOs.Payment;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services.Providers;

public class SePayProvider : IPaymentProvider
{
  private readonly IConfiguration _config;

  public SePayProvider(IConfiguration config)
  {
    _config = config;
  }

  public string ProviderName => "SEPAY";

  public Task<string> CreatePaymentUrlAsync(PaymentRequestDto request)
  {
    var frontendUrl = _config["App:FrontendUrl"] ?? throw new InvalidOperationException("App:FrontendUrl is missing.");
    var paymentIdStr = request.PaymentId.ToString("N");
    
    // Instead of redirecting directly to an image, redirect to our frontend checkout page.
    // The frontend checkout page will render the QR code using qr.sepay.vn.
    var checkoutUrl = $"{frontendUrl}/payment/sepay?paymentId={request.PaymentId}&amount={request.Amount}&des={paymentIdStr}";
    
    return Task.FromResult(checkoutUrl);
  }

  public Task<PaymentResultDto> VerifyCallbackAsync(IQueryCollection query)
  {
    // SePay Webhook is typically a POST request with JSON body.
    // It's handled explicitly in PaymentController and PaymentService.
    // This method is just a fallback to satisfy IPaymentProvider interface
    // for standard GET callbacks, which SePay doesn't use.
    throw new NotSupportedException("SePay uses POST webhooks. Please use ProcessSePayWebhookAsync instead.");
  }
}
