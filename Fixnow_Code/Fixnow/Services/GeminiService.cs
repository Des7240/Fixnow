using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Fixnow.DTOs.AISupport;
using Fixnow.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace Fixnow.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private const string GeminiModel = "gemini-3-flash-preview";

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        /// <summary>
        /// Analyzes a problem description and an optional image using Google Gemini AI.
        /// </summary>
        /// <param name="request">The AI support request containing description and image.</param>
        /// <returns>The AI response.</returns>
        public async Task<AISupportResponseDto> AnalyzeProblemAsync(AISupportRequestDto request)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Gemini API Key is not configured.");
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent?key={_apiKey}";

            var parts = new List<object>
            {
                new { text = $"You are an AI support assistant for a repair service platform. A customer is reporting a problem. Analyze the problem and provide helpful advice, possible causes, or next steps. \nCustomer problem: {request.ProblemDescription}" }
            };

            if (request.Image != null && request.Image.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await request.Image.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = request.Image.ContentType;

                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = mimeType,
                        data = base64Image
                    }
                });
            }

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = parts
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to communicate with Gemini API. Status: {response.StatusCode}. Details: {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();

            try
            {
                var generatedText = responseData
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return new AISupportResponseDto
                {
                    ResponseText = generatedText ?? "No response generated."
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse the response from Gemini API.", ex);
            }
        }
    }
}
