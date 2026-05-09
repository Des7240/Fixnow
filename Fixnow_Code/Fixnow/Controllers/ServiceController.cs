using Fixnow.Entities;
using Fixnow.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// Service categories management: list and create services.
/// </summary>
[ApiController]
[Route("api/v1/services")]
public class ServiceController : ControllerBase
{
  private readonly IServiceCategoryRepository _serviceRepo;

  public ServiceController(IServiceCategoryRepository serviceRepo)
  {
    _serviceRepo = serviceRepo;
  }

  /// <summary>List all active service categories.</summary>
  [HttpGet]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAll()
  {
    var services = await _serviceRepo.FindAllActiveAsync();
    var result = services.Select(s => new
    {
      id = s.Id,
      name = s.Name,
      description = s.Description,
      iconUrl = s.IconUrl,
      basePrice = s.BasePrice,
      estimatedDurationMinutes = s.EstimatedDurationMinutes
    });
    return Ok(result);
  }

  /// <summary>GET /api/v1/services/search?q={keyword} — Search services</summary>
  [HttpGet("search")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Search([FromQuery(Name = "q")] string keyword)
  {
    if (string.IsNullOrWhiteSpace(keyword))
      return await GetAll();

    var services = await _serviceRepo.SearchAsync(keyword);
    var result = services.Select(s => new
    {
      id = s.Id,
      name = s.Name,
      description = s.Description,
      iconUrl = s.IconUrl,
      basePrice = s.BasePrice,
      estimatedDurationMinutes = s.EstimatedDurationMinutes
    });
    return Ok(result);
  }

  /// <summary>GET /api/v1/services/{id} — Get service details</summary>
  [HttpGet("{id:guid}")]
  [AllowAnonymous]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> GetById(Guid id)
  {
    var s = await _serviceRepo.FindByIdAsync(id);
    if (s == null) return NotFound(new { message = "Service not found." });

    return Ok(new
    {
      id = s.Id,
      name = s.Name,
      description = s.Description,
      iconUrl = s.IconUrl,
      basePrice = s.BasePrice,
      estimatedDurationMinutes = s.EstimatedDurationMinutes,
      isActive = s.IsActive
    });
  }

  /// <summary>Create a new service category (Admin only).</summary>
  [HttpPost]
  [Authorize(Roles = "ADMIN")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<IActionResult> Create([FromBody] CreateServiceDto request)
  {
    var service = new ServiceCategory
    {
      Name = request.Name,
      Description = request.Description,
      IconUrl = request.IconUrl,
      BasePrice = request.BasePrice,
      EstimatedDurationMinutes = request.EstimatedDurationMinutes
    };

    var created = await _serviceRepo.CreateAsync(service);
    return StatusCode(StatusCodes.Status201Created, new
    {
      id = created.Id,
      name = created.Name,
      basePrice = created.BasePrice
    });
  }
}

/// <summary>DTO for creating a service category.</summary>
public record CreateServiceDto(string Name, string? Description, string? IconUrl, decimal BasePrice = 0, int EstimatedDurationMinutes = 60);
