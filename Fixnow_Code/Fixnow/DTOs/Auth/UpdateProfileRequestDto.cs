namespace Fixnow.DTOs.Auth;

public record UpdateProfileRequestDto(
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl
);
