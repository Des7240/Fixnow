using Fixnow.DTOs.Payment;
using Fixnow.DTOs.Wallet;
using Fixnow.Entities;
using Fixnow.Enums;
using Fixnow.Repositories.Interfaces;
using Fixnow.Services.Interfaces;
using Fixnow.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Fixnow.Services;

public class PaymentService : IPaymentService
{
  private readonly IPaymentRepository _paymentRepo;
  private readonly IBookingRepository _bookingRepo;
  private readonly IAuditService _auditService;
  private readonly IConfiguration _config;
  private readonly IEnumerable<IPaymentProvider> _providers;
  private readonly IWalletService _walletService;
  private readonly IHubContext<NotificationHub> _hubContext;

  public PaymentService(
    IPaymentRepository paymentRepo,
    IBookingRepository bookingRepo,
    IAuditService auditService,
    IConfiguration config,
    IEnumerable<IPaymentProvider> providers,
    IWalletService walletService,
    IHubContext<NotificationHub> hubContext)
  {
    _paymentRepo = paymentRepo;
    _bookingRepo = bookingRepo;
    _auditService = auditService;
    _config = config;
    _providers = providers;
    _walletService = walletService;
    _hubContext = hubContext;
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

    var baseAmount = booking.TotalAmount ?? 0;
    if (baseAmount <= 0)
      throw new InvalidOperationException("Invalid booking amount. Quotation might not be approved.");

    var discountAmount = booking.DiscountAmount;
    var amountToPay = baseAmount - discountAmount;
    if (amountToPay < 0) amountToPay = 0;

    var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(request.Provider.ToString(), StringComparison.InvariantCultureIgnoreCase))
      ?? throw new NotSupportedException($"Provider {request.Provider} not supported.");

    var payment = new Payment
    {
      BookingId = booking.Id,
      CustomerId = customerId,
      Provider = request.Provider,
      Amount = amountToPay,
      Status = PaymentStatus.PENDING,
      Type = PaymentType.BOOKING
    };

    await _paymentRepo.CreateAsync(payment);

    var apiBaseUrl = _config["App:ApiBaseUrl"] 
        ?? throw new InvalidOperationException("App:ApiBaseUrl is not configured.");
    var returnUrl = $"{apiBaseUrl}/api/v1/payments/{request.Provider.ToString().ToLower()}/callback";

    var paymentRequest = new PaymentRequestDto
    {
      PaymentId = payment.Id,
      BookingId = booking.Id,
      Amount = amountToPay,
      Description = $"Payment for booking {booking.Id}",
      ReturnUrl = returnUrl,
      IpAddress = ipAddress
    };

    var paymentUrl = await provider.CreatePaymentUrlAsync(paymentRequest);

    await _auditService.LogActionAsync("PAYMENT_CREATED", "Payment", customerId, "CUSTOMER", payment.Id, null, $"{{ \"amount\": {amountToPay}, \"provider\": \"{request.Provider}\" }}");

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

    var apiBaseUrl = _config["App:ApiBaseUrl"] 
        ?? throw new InvalidOperationException("App:ApiBaseUrl is not configured.");
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

          // Worker income is calculated based on the original TotalAmount, not the discounted Payment Amount.
          // Because Admin (Fixnow) sponsors the voucher discount.
          var baseAmount = booking.TotalAmount ?? 0;
          var platformFee = baseAmount * 0.1m;
          var workerIncome = baseAmount - platformFee;

          var financial = new BookingFinancial
          {
            BookingId = booking.Id,
            TotalAmount = payment.Amount, // Customer paid amount
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

  public async Task<PaymentResultDto> ProcessSePayWebhookAsync(SePayWebhookDto payload)
  {
    var rawResponse = System.Text.Json.JsonSerializer.Serialize(payload);
    
    // We assume the payload contains the PaymentId in the "content" field without hyphens.
    // Let's try to extract 32 hex chars from content.
    var match = System.Text.RegularExpressions.Regex.Match(payload.Content, @"[a-fA-F0-9]{32}");
    if (!match.Success)
    {
      var failedResult = new PaymentResultDto { IsSuccess = false, RawResponse = rawResponse, ErrorMessage = "No PaymentId found in content" };
      await LogCallbackRawAsync("SEPAY", failedResult, false);
      return failedResult;
    }

    var paymentIdStr = match.Value;
    if (!Guid.TryParseExact(paymentIdStr, "N", out var paymentId))
    {
      var failedResult = new PaymentResultDto { IsSuccess = false, RawResponse = rawResponse, ErrorMessage = "Invalid PaymentId format" };
      await LogCallbackRawAsync("SEPAY", failedResult, false);
      return failedResult;
    }

    var payment = await _paymentRepo.FindByIdWithDetailsAsync(paymentId);
    if (payment == null)
    {
      var failedResult = new PaymentResultDto { IsSuccess = false, RawResponse = rawResponse, ErrorMessage = "Payment not found" };
      await LogCallbackRawAsync("SEPAY", failedResult, false);
      return failedResult;
    }

    // Amount check (optional depending on business logic, here we just check if it's equal or greater)
    if (payload.TransferAmount < payment.Amount)
    {
      var failedResult = new PaymentResultDto { IsSuccess = false, RawResponse = rawResponse, ErrorMessage = "Transferred amount is less than expected" };
      await LogCallbackRawAsync("SEPAY", failedResult, true);
      return failedResult;
    }

    var result = new PaymentResultDto
    {
      IsSuccess = true,
      TransactionId = payload.ReferenceCode,
      RawResponse = rawResponse,
      Amount = payload.TransferAmount
    };

    await LogCallbackRawAsync("SEPAY", result, true);

    if (payment.Status == PaymentStatus.SUCCESS || payment.Status == PaymentStatus.FAILED)
      return result;

    var transaction = new Transaction
    {
      PaymentId = payment.Id,
      GatewayTransactionId = result.TransactionId,
      ProviderResponse = result.RawResponse,
      Status = PaymentStatus.SUCCESS
    };
    await _paymentRepo.AddTransactionAsync(transaction);

    payment.Status = PaymentStatus.SUCCESS;
    payment.TransactionCode = result.TransactionId;
    await _paymentRepo.UpdateAsync(payment);

    if (payment.Type == PaymentType.BOOKING && payment.BookingId.HasValue)
    {
      var booking = await _bookingRepo.FindByIdAsync(payment.BookingId.Value);
      if (booking != null)
      {
        booking.PaymentStatus = BookingPaymentStatus.PAID;
        await _bookingRepo.UpdateAsync(booking);

        var baseAmount = booking.TotalAmount ?? 0;
        var platformFee = baseAmount * 0.1m;
        var workerIncome = baseAmount - platformFee;

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

    // Push SignalR notification to the user
    await _hubContext.Clients.User(payment.CustomerId.ToString()).SendAsync("ReceivePaymentSuccess", payment.Id);

    return result;
  }

  public async Task<PaymentStatusResponseDto> GetPaymentStatusAsync(Guid paymentId, Guid currentUserId)
  {
    var payment = await _paymentRepo.FindByIdAsync(paymentId);
    if (payment == null)
      throw new KeyNotFoundException("Payment not found");

    if (payment.CustomerId != currentUserId)
      throw new UnauthorizedAccessException("You are not authorized to view this payment");

    return new PaymentStatusResponseDto { Status = payment.Status };
  }
}
