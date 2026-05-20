using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fixnow.DTOs.AISupport;
using Fixnow.Services.Interfaces;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Fixnow.Controllers
{
    [Route("api/v1/AISupport")]
    [ApiController]
    // [Authorize] // Uncomment this if only authenticated users can use the AI support
    public class AISupportController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public AISupportController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        /// <summary>
        /// Gets AI support based on problem description and optional image.
        /// </summary>
        /// <param name="request">Request containing problem description and image file.</param>
        /// <returns>AI analysis response.</returns>
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AISupportResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Analyze([FromForm] AISupportRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _geminiService.AnalyzeProblemAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // In a real application, you should log the exception
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while communicating with the AI service.", details = ex.Message });
            }
        }
    }
}
