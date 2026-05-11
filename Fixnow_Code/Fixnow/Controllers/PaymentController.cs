using Fixnow.DTOs.Payment;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentController : ControllerBase
{
  private readonly IPaymentService _paymentService;
  private readonly IConfiguration _config;

  public PaymentController(IPaymentService paymentService, IConfiguration config)
  {
    _paymentService = paymentService;
    _config = config;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  [HttpPost("vnpay")]
  [Authorize]
  public async Task<IActionResult> CreateVNPayPayment([FromBody] CreatePaymentRequestDto request)
  {
    request.Provider = Enums.PaymentProvider.VNPAY;
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var result = await _paymentService.CreatePaymentAsync(request, CurrentUserId, ipAddress);
    return Ok(result);
  }

  [HttpPost("momo")]
  [Authorize]
  public async Task<IActionResult> CreateMoMoPayment([FromBody] CreatePaymentRequestDto request)
  {
    request.Provider = Enums.PaymentProvider.MOMO;
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var result = await _paymentService.CreatePaymentAsync(request, CurrentUserId, ipAddress);
    return Ok(result);
  }

  [HttpGet("vnpay/callback")]
  [AllowAnonymous]
  public async Task<IActionResult> VNPayCallback()
  {
    var result = await _paymentService.ProcessCallbackAsync("VNPAY", Request.Query);
    
    var frontendUrl = _config["App:FrontendUrl"] ?? throw new InvalidOperationException("App:FrontendUrl is not configured.");
    return Redirect($"{frontendUrl}/payment/result?success={result.IsSuccess}&provider=vnpay");
  }

  [HttpGet("momo/callback")]
  [AllowAnonymous]
  public async Task<IActionResult> MoMoCallback()
  {
    var result = await _paymentService.ProcessCallbackAsync("MOMO", Request.Query);
    
    var frontendUrl = _config["App:FrontendUrl"] ?? throw new InvalidOperationException("App:FrontendUrl is not configured.");
    return Redirect($"{frontendUrl}/payment/result?success={result.IsSuccess}&provider=momo");
  }
}
