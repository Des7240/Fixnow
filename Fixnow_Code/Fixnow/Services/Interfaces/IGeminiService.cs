using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Fixnow.DTOs.AISupport;

namespace Fixnow.Services.Interfaces
{
    public interface IGeminiService
    {
        /// <summary>
        /// Analyzes a problem description and an optional image using Google Gemini AI.
        /// </summary>
        /// <param name="request">The AI support request containing description and image.</param>
        /// <returns>The AI response.</returns>
        Task<AISupportResponseDto> AnalyzeProblemAsync(AISupportRequestDto request);
    }
}
