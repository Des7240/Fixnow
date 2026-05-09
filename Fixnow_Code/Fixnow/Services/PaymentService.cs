using Fixnow.DTOs.Payment;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;

namespace Fixnow.Services;

public class PaymentService : IPaymentService
{
  private readonly IPaymentRepository _paymentRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly IAuditService _auditService;
  private readonly IConfiguration _config;
  private readonly IEnumerable<IPaymentProvider> _providers;
  private readonly IWalletService _walletService;

  public PaymentService(
    IPaymentRepository paymentRepo,
    IBookingRepository bookingRepo,
    IAuditService auditService,
    IConfiguration config,
    IEnumerable<IPaymentProvider> providers,
    IWalletService walletService)
  {
    _paymentRepo = paymentRepo;
    _bookingRepo = bookingRepo;
    _auditService = auditService;
    _config = config;
    _providers = providers;
    _walletService = walletService;
  }

  public async Task<CreatePaymentResponseDto> CreatePaymentAsync(CreatePaymentRequestDto request, Guid customerId, string ipAddress)
  {
    var booking = await _bookingRepo.FindByIdWithDetailsAsync(request.BookingId)
      ?? throw new KeyNotFoundException("Booking not found.");

    if (booking.CustomerId != customerId)
      throw new UnauthorizedAccessException("You don't own this booking.");

    if (booking.Status != BookingStatus.WORKING && booking.Status != BookingStatus.COMPLETED)
      throw new InvalidOperationException("Booking must be APPROVED or COMPLETED before payment.");

    if (booking.PaymentStatus == BookingPaymentStatus.PAID)
      throw new InvalidOperationException("Booking is already PAID.");

    var amount = booking.TotalAmount ?? 0;
    if (amount <= 0)
      throw new InvalidOperationException("Invalid booking amount. Quotation might not be approved.");

    var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(request.Provider.ToString(), StringComparison.InvariantCultureIgnoreCase))
      ?? throw new NotSupportedException($"Provider {request.Provider} not supported.");

    var payment = new Payment
    {
      BookingId = booking.Id,
      CustomerId = customerId,
      Provider = request.Provider,
      Amount = amount,
      Status = PaymentStatus.PENDING
    };

    await _paymentRepo.CreateAsync(payment);

    var baseUrl = _config["App:FrontendUrl"] ?? "http://localhost:5173";
    var returnUrl = $"{baseUrl}/payment/callback/{request.Provider.ToString().ToLower()}";

    var paymentRequest = new PaymentRequestDto
    {
      PaymentId = payment.Id,
      BookingId = booking.Id,
      Amount = amount,
      Description = $"Payment for booking {booking.Id}",
      ReturnUrl = returnUrl,
      IpAddress = ipAddress
    };

    var paymentUrl = await provider.CreatePaymentUrlAsync(paymentRequest);

    await _auditService.LogActionAsync("PAYMENT_CREATED", "Payment", customerId, "CUSTOMER", payment.Id, null, $"{{ \"amount\": {amount}, \"provider\": \"{request.Provider}\" }}");

    return new CreatePaymentResponseDto
    {
      PaymentId = payment.Id,
      PaymentUrl = paymentUrl
    };
  }

  public async Task<PaymentResultDto> ProcessCallbackAsync(string providerName, IQueryCollection query)
  {
    var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.InvariantCultureIgnoreCase))
      ?? throw new NotSupportedException($"Provider {providerName} not supported.");

    var result = await provider.VerifyCallbackAsync(query);

    // Parse paymentId
    // For VNPay, it's typically vnp_TxnRef. For MoMo, it could be extracted.
    // To simplify, if we cannot get it, we just return the result.
    var paymentIdStr = providerName.ToUpper() == "VNPAY" ? query["vnp_TxnRef"].ToString() : query["paymentId"].ToString();
    if (!Guid.TryParse(paymentIdStr, out var paymentId))
    {
      await LogCallbackRawAsync(providerName, result, false);
      return result;
    }

    var payment = await _paymentRepo.FindByIdWithDetailsAsync(paymentId);
    if (payment == null)
    {
      await LogCallbackRawAsync(providerName, result, false);
      return result; // Invalid payment
    }

    await LogCallbackRawAsync(providerName, result, result.IsSuccess);

    // Idempotency check
    if (payment.Status == PaymentStatus.SUCCESS || payment.Status == PaymentStatus.FAILED)
      return result;

    var transaction = new Transaction
    {
      PaymentId = payment.Id,
      GatewayTransactionId = result.TransactionId,
      ProviderResponse = result.RawResponse,
      Status = result.IsSuccess ? PaymentStatus.SUCCESS : PaymentStatus.FAILED
    };
    await _paymentRepo.AddTransactionAsync(transaction);

    payment.Status = result.IsSuccess ? PaymentStatus.SUCCESS : PaymentStatus.FAILED;
    payment.TransactionCode = result.TransactionId;
    await _paymentRepo.UpdateAsync(payment);

    if (result.IsSuccess)
    {
      var booking = await _bookingRepo.FindByIdAsync(payment.BookingId);
      if (booking != null)
      {
        booking.PaymentStatus = BookingPaymentStatus.PAID;
        await _bookingRepo.UpdateAsync(booking);

        // Calculate financials
        var platformFee = payment.Amount * 0.1m; // 10%
        var workerIncome = payment.Amount - platformFee;

        var financial = new BookingFinancial
        {
          BookingId = booking.Id,
          TotalAmount = payment.Amount,
          PlatformFee = platformFee,
          WorkerIncome = workerIncome
        };
        await _paymentRepo.CreateFinancialAsync(financial);

        // Process wallet
        await _walletService.ProcessBookingIncomeAsync(booking.Id);

        await _auditService.LogActionAsync("PAYMENT_SUCCESS", "Payment", null, "SYSTEM", payment.Id, null, $"{{ \"amount\": {payment.Amount} }}");
      }
    }
    else
    {
      await _auditService.LogActionAsync("PAYMENT_FAILED", "Payment", null, "SYSTEM", payment.Id, null, $"{{ \"error\": \"{result.ErrorMessage}\" }}");
    }

    return result;
  }

  private async Task LogCallbackRawAsync(string provider, PaymentResultDto result, bool verified)
  {
    var callback = new PaymentCallback
    {
      Provider = Enum.Parse<PaymentProvider>(provider, true),
      Payload = result.RawResponse,
      Verified = verified
    };
    await _paymentRepo.AddCallbackAsync(callback);
    await _auditService.LogActionAsync("PAYMENT_CALLBACK_RECEIVED", "PaymentCallback", null, "SYSTEM", callback.Id, null, result.RawResponse);
  }
}
