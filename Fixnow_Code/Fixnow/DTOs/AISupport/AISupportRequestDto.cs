using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.AISupport
{
    public class AISupportRequestDto
    {
        [Required(ErrorMessage = "Problem description is required")]
        public string ProblemDescription { get; set; } = string.Empty;

        public IFormFile? Image { get; set; }
    }
}
