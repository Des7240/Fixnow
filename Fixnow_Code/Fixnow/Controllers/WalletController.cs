using System.Security.Claims;
using Fixnow.DTOs.Wallet;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/wallet")]
[Authorize] // All wallet operations require authentication
public class WalletController : ControllerBase
{
  private readonly IWalletService _walletService;
  private readonly IPaymentService _paymentService;

  public WalletController(IWalletService walletService, IPaymentService paymentService)
  {
    _walletService = walletService;
    _paymentService = paymentService;
  }

  /// <summary>
  /// Lấy thông tin ví của người dùng hiện tại (chủ yếu là Worker).
  /// </summary>
  [HttpGet]
  [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMyWallet()
  {
    var userId = GetCurrentUserId();
    var wallet = await _walletService.GetWalletAsync(userId);
    return Ok(wallet);
  }

  /// <summary>
  /// Lấy danh sách biến động số dư (Transaction Ledger).
  /// </summary>
  [HttpGet("transactions")]
  [ProducesResponseType(typeof(List<WalletTransactionDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMyTransactions()
  {
    var userId = GetCurrentUserId();
    var transactions = await _walletService.GetTransactionsAsync(userId);
    return Ok(transactions);
  }

  /// <summary>
  /// Lấy lịch sử yêu cầu rút tiền.
  /// </summary>
  [HttpGet("withdrawals")]
  [ProducesResponseType(typeof(List<WithdrawalDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMyWithdrawals()
  {
    var userId = GetCurrentUserId();
    var withdrawals = await _walletService.GetWithdrawalsAsync(userId);
    return Ok(withdrawals);
  }

  /// <summary>
  /// Khởi tạo yêu cầu rút tiền (Gửi OTP qua email).
  /// </summary>
  [HttpPost("withdraw")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> RequestWithdrawal([FromBody] WithdrawRequestDto request)
  {
    var userId = GetCurrentUserId();
    try
    {
      await _walletService.InitiateWithdrawalAsync(userId, request);
      return Ok(new { message = "Mã OTP đã được gửi về email của bạn." });
    }
    catch (Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>
  /// Xác nhận rút tiền với mã OTP.
  /// </summary>
  [HttpPost("confirm-withdraw")]
  [ProducesResponseType(typeof(WithdrawalDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ConfirmWithdrawal([FromBody] ConfirmWithdrawRequestDto request)
  {
    var userId = GetCurrentUserId();
    try
    {
      var withdrawal = await _walletService.ConfirmWithdrawalAsync(userId, request);
      return Ok(withdrawal);
    }
    catch (Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>
  /// Tạo yêu cầu nạp tiền vào ví.
  /// </summary>
  [HttpPost("deposit")]
  public async Task<IActionResult> CreateDeposit([FromBody] CreateWalletDepositRequestDto request)
  {
    var userId = GetCurrentUserId();
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    try
    {
      var result = await _paymentService.CreateWalletDepositAsync(request, userId, ipAddress);
      return Ok(result);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  private Guid GetCurrentUserId()
  {
    var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return Guid.Parse(idStr!);
  }
}
