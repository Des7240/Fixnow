using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Auth;

public record GoogleLoginRequestDto(
    [Required] string IdToken
);
