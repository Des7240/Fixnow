using Fixnow.DTOs.Promotion;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

[ApiController]
[Route("api/v1/promotions")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet("active")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<PromotionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePromotions()
    {
        var result = await _promotionService.GetActivePromotionsAsync();
        return Ok(result);
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidatePromotionResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePromotion([FromBody] ValidatePromotionDto request)
    {
        var result = await _promotionService.ValidatePromotionAsync(request);
        return Ok(result);
    }

    // --- Admin endpoints ---

    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(List<PromotionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPromotions()
    {
        var result = await _promotionService.GetAllPromotionsAsync();
        return Ok(result);
    }

    [HttpPost("admin")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePromotion([FromBody] CreatePromotionDto request)
    {
        try
        {
            var result = await _promotionService.CreatePromotionAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("admin/{id:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePromotionStatus([FromRoute] Guid id, [FromBody] bool isActive)
    {
        var result = await _promotionService.UpdatePromotionStatusAsync(id, isActive);
        return Ok(result);
    }
}
