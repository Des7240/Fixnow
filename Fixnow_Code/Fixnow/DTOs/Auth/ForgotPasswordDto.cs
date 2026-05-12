using System.ComponentModel.DataAnnotations;

namespace Fixnow.DTOs.Auth;

public record ForgotPasswordRequestDto([Required][EmailAddress] string Email);

public record VerifyOtpRequestDto(
    [Required][EmailAddress] string Email,
    [Required] string Code
);

public record ResetPasswordRequestDto(
    [Required][EmailAddress] string Email,
    [Required] string Code,
    [Required][MinLength(6)] string NewPassword
);
