using Fixnow.DTOs.Quotation;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/quotations")]
[Authorize]
public class QuotationController : ControllerBase
{
  private readonly IQuotationService _quotationService;

  public QuotationController(IQuotationService quotationService)
  {
    _quotationService = quotationService;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  [HttpPost]
  public async Task<IActionResult> CreateQuotation([FromBody] CreateQuotationRequestDto request)
  {
    var result = await _quotationService.CreateQuotationAsync(request, CurrentUserId);
    return Ok(result);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetQuotation(Guid id)
  {
    var result = await _quotationService.GetQuotationAsync(id);
    return Ok(result);
  }

  [HttpGet("booking/{bookingId}")]
  public async Task<IActionResult> GetQuotationsByBooking(Guid bookingId)
  {
    var results = await _quotationService.GetQuotationsByBookingAsync(bookingId);
    return Ok(results);
  }

  [HttpPost("{id}/approve")]
  public async Task<IActionResult> ApproveQuotation(Guid id)
  {
    var result = await _quotationService.ApproveQuotationAsync(id, CurrentUserId);
    return Ok(result);
  }

  [HttpPost("{id}/reject")]
  public async Task<IActionResult> RejectQuotation(Guid id)
  {
    var result = await _quotationService.RejectQuotationAsync(id, CurrentUserId);
    return Ok(result);
  }
}
