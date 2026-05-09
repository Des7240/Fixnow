using System.Security.Claims;
using Fixnow.DTOs.File;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fixnow.Controllers;

/// <summary>
/// API for file storage management.
/// </summary>
[ApiController]
[Route("api/v1/files")]
[Authorize]
public class FileController : ControllerBase
{
  private readonly IFileService _fileService;

  public FileController(IFileService fileService)
  {
    _fileService = fileService;
  }

  private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

  /// <summary>POST /api/v1/files/upload — Upload a file.</summary>
  [HttpPost("upload")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<ActionResult<FileResponseDto>> UploadFile(IFormFile file, [FromQuery] string bucket = "local")
  {
    try
    {
      var uploaded = await _fileService.UploadFileAsync(file, CurrentUserId, bucket);
      var response = new FileResponseDto
      {
        Id = uploaded.Id,
        FileName = uploaded.FileName,
        ObjectKey = uploaded.ObjectKey,
        ContentType = uploaded.ContentType,
        FileSize = uploaded.FileSize,
        CreatedAt = uploaded.CreatedAt
      };
      return CreatedAtAction(nameof(GetFileMetadata), new { id = uploaded.Id }, response);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>GET /api/v1/files/{id} — Get file metadata.</summary>
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<ActionResult<FileResponseDto>> GetFileMetadata(Guid id)
  {
    var file = await _fileService.GetFileMetadataAsync(id);
    if (file == null) return NotFound();

    return Ok(new FileResponseDto
    {
      Id = file.Id,
      FileName = file.FileName,
      ObjectKey = file.ObjectKey,
      ContentType = file.ContentType,
      FileSize = file.FileSize,
      CreatedAt = file.CreatedAt
    });
  }
}
