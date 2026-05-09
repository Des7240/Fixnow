using Fixnow.DTOs.Payment;
using Fixnow.DTOs.Wallet;
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
      Status = PaymentStatus.PENDING,
      Type = PaymentType.BOOKING
    };

    await _paymentRepo.CreateAsync(payment);

    var apiBaseUrl = _config["App:ApiBaseUrl"] ?? "https://localhost:7154";
    var returnUrl = $"{apiBaseUrl}/api/v1/payments/{request.Provider.ToString().ToLower()}/callback";

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

  public async Task<CreatePaymentResponseDto> CreateWalletDepositAsync(CreateWalletDepositRequestDto request, Guid userId, string ipAddress)
  {
    if (request.Amount < 10000) // Lowered from 50k for testing flexibility
      throw new ArgumentException("Minimum deposit amount is 10,000 VND.");

    var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(request.Provider.ToString(), StringComparison.InvariantCultureIgnoreCase))
      ?? throw new NotSupportedException($"Provider {request.Provider} not supported.");

    var payment = new Payment
    {
      CustomerId = userId,
      Provider = request.Provider,
      Amount = request.Amount,
      Status = PaymentStatus.PENDING,
      Type = PaymentType.WALLET_DEPOSIT
    };

    await _paymentRepo.CreateAsync(payment);

    var apiBaseUrl = _config["App:ApiBaseUrl"] ?? "https://localhost:7154";
    var returnUrl = $"{apiBaseUrl}/api/v1/payments/{request.Provider.ToString().ToLower()}/callback";

    var paymentRequest = new PaymentRequestDto
    {
      PaymentId = payment.Id,
      Amount = request.Amount,
      Description = $"Wallet deposit top-up",
      ReturnUrl = returnUrl,
      IpAddress = ipAddress
    };

    var paymentUrl = await provider.CreatePaymentUrlAsync(paymentRequest);

    await _auditService.LogActionAsync("DEPOSIT_CREATED", "Payment", userId, "USER", payment.Id, null, $"{{ \"amount\": {request.Amount}, \"provider\": \"{request.Provider}\" }}");

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
      return result;
    }

    await LogCallbackRawAsync(providerName, result, result.IsSuccess);

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
      if (payment.Type == PaymentType.BOOKING && payment.BookingId.HasValue)
      {
        var booking = await _bookingRepo.FindByIdAsync(payment.BookingId.Value);
        if (booking != null)
        {
          booking.PaymentStatus = BookingPaymentStatus.PAID;
          await _bookingRepo.UpdateAsync(booking);

          var platformFee = payment.Amount * 0.1m;
          var workerIncome = payment.Amount - platformFee;

          var financial = new BookingFinancial
          {
            BookingId = booking.Id,
            TotalAmount = payment.Amount,
            PlatformFee = platformFee,
            WorkerIncome = workerIncome
          };
          await _paymentRepo.CreateFinancialAsync(financial);

          await _walletService.ProcessBookingIncomeAsync(booking.Id);
        }
      }
      else if (payment.Type == PaymentType.WALLET_DEPOSIT)
      {
        await _walletService.ProcessDepositAsync(payment.Id);
      }

      await _auditService.LogActionAsync("PAYMENT_SUCCESS", "Payment", null, "SYSTEM", payment.Id, null, $"{{ \"amount\": {payment.Amount}, \"type\": \"{payment.Type}\" }}");
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
