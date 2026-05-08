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
    });
    return Ok(result);
  }

  /// <summary>Create a new service category (Admin only).</summary>
  [HttpPost]
  //[Authorize(Roles = "ADMIN")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<IActionResult> Create([FromBody] CreateServiceDto request)
  {
    var service = new ServiceCategory
    {
      Name = request.Name,
      Description = request.Description,
      IconUrl = request.IconUrl,
    };

    var created = await _serviceRepo.CreateAsync(service);
    return StatusCode(StatusCodes.Status201Created, new
    {
      id = created.Id,
      name = created.Name,
    });
  }
}

/// <summary>DTO for creating a service category.</summary>
public record CreateServiceDto(string Name, string? Description, string? IconUrl);
