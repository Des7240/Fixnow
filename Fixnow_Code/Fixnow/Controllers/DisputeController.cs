using System.Security.Claims;
using Fixnow.DTOs.Dispute;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/disputes")]
[Authorize]
public class DisputeController : ControllerBase
{
  private readonly IDisputeService _disputeService;

  public DisputeController(IDisputeService disputeService)
  {
    _disputeService = disputeService;
  }

  /// <summary>Khởi tạo khiếu nại (Dành cho Customer)</summary>
  [HttpPost]
  [ProducesResponseType(typeof(DisputeDto), StatusCodes.Status201Created)]
  public async Task<IActionResult> CreateDispute([FromBody] CreateDisputeDto request)
  {
    var userId = GetCurrentUserId();
    try
    {
      var dispute = await _disputeService.CreateDisputeAsync(userId, request);
      return StatusCode(StatusCodes.Status201Created, dispute);
    }
    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>Upload bằng chứng cho khiếu nại</summary>
  [HttpPost("{id}/evidences")]
  [ProducesResponseType(typeof(DisputeEvidenceDto), StatusCodes.Status201Created)]
  public async Task<IActionResult> UploadEvidence(Guid id, IFormFile file)
  {
    var userId = GetCurrentUserId();
    try
    {
      var evidence = await _disputeService.AddEvidenceAsync(id, userId, file);
      return StatusCode(StatusCodes.Status201Created, evidence);
    }
    catch (Exception ex) when (ex is UnauthorizedAccessException || ex is InvalidOperationException)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>Xem chi tiết khiếu nại</summary>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(DisputeDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetDispute(Guid id)
  {
    var dispute = await _disputeService.GetDisputeAsync(id);
    return Ok(dispute);
  }

  /// <summary>Lấy danh sách khiếu nại của tôi</summary>
  [HttpGet]
  [ProducesResponseType(typeof(List<DisputeDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetMyDisputes()
  {
    var userId = GetCurrentUserId();
    var disputes = await _disputeService.GetMyDisputesAsync(userId);
    return Ok(disputes);
  }

  // --- ADMIN APIs ---

  /// <summary>Lấy tất cả khiếu nại (Admin)</summary>
  [HttpGet("admin/all")]
  [Authorize(Roles = "ADMIN")]
  public async Task<IActionResult> GetAllDisputes()
  {
    var disputes = await _disputeService.GetAllDisputesAsync();
    return Ok(disputes);
  }

  /// <summary>Hoàn tiền cho khách (Admin). Sẽ trừ tiền từ ví Thợ</summary>
  [HttpPost("admin/{id}/refund")]
  [Authorize(Roles = "ADMIN")]
  public async Task<IActionResult> ProcessRefund(Guid id, [FromBody] RefundRequestDto request)
  {
    var adminId = GetCurrentUserId();
    try
    {
      var dispute = await _disputeService.ProcessRefundAsync(adminId, id, request);
      return Ok(dispute);
    }
    catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>Đóng khiếu nại (Admin)</summary>
  [HttpPost("admin/{id}/close")]
  [Authorize(Roles = "ADMIN")]
  public async Task<IActionResult> CloseDispute(Guid id)
  {
    var adminId = GetCurrentUserId();
    var dispute = await _disputeService.CloseDisputeAsync(adminId, id);
    return Ok(dispute);
  }

  private Guid GetCurrentUserId()
  {
    var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return Guid.Parse(idStr!);
  }
}
