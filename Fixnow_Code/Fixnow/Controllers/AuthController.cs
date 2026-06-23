using System.Security.Claims;
using Fixnow.DTOs.Auth;
using Fixnow.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Fixnow.Controllers;

/// <summary>
/// Authentication controller handling register, login, refresh token, and logout.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;

  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  /// <summary>Register a new user account.</summary>
  [HttpPost("register")]
  [EnableRateLimiting("otp-policy")]
  [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
  {
    var result = await _authService.RegisterAsync(request);
    return StatusCode(StatusCodes.Status201Created, result);
  }

  /// <summary>Login with email and password.</summary>
  [HttpPost("login")]
  [EnableRateLimiting("login-policy")]
  [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
  {
    var result = await _authService.LoginAsync(request);

    // Set refresh token as HttpOnly cookie
    SetRefreshTokenCookie(result.RefreshToken);

    // Clear refresh token from body (security: keep in cookie only)
    // For mobile app support, we need to return it in the body.
    // result.RefreshToken = string.Empty;

    return Ok(result);
  }

  /// <summary>Login with Google ID Token.</summary>
  [HttpPost("google-login")]
  [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
  {
    var result = await _authService.GoogleLoginAsync(request);

    // Set refresh token as HttpOnly cookie
    SetRefreshTokenCookie(result.RefreshToken);

    // Clear refresh token from body
    // result.RefreshToken = string.Empty;

    return Ok(result);
  }

  /// <summary>Refresh the access token using the refresh token cookie.</summary>
  [HttpPost("refresh")]
  [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto? request)
  {
    // Accept refresh token from cookie first, then fallback to body
    var refreshToken = Request.Cookies["refreshToken"]
      ?? request?.RefreshToken
      ?? throw new UnauthorizedAccessException("Refresh token not provided.");

    var result = await _authService.RefreshTokenAsync(refreshToken);

    SetRefreshTokenCookie(result.RefreshToken);
    // result.RefreshToken = string.Empty;

    return Ok(result);
  }

  /// <summary>Logout and revoke the refresh token.</summary>
  [HttpPost("logout")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> Logout()
  {
    var refreshToken = Request.Cookies["refreshToken"];
    if (!string.IsNullOrEmpty(refreshToken))
      await _authService.LogoutAsync(refreshToken);

    // Clear cookie
    Response.Cookies.Delete("refreshToken");

    return Ok(new { message = "Logged out successfully." });
  }

  /// <summary>Change current user's password.</summary>
  [HttpPost("change-password")]
  [Authorize]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
  {
    var userId = GetCurrentUserId();
    await _authService.ChangePasswordAsync(userId, request);
    return Ok(new { message = "Đổi mật khẩu thành công." });
  }

  /// <summary>Request a password reset OTP via email.</summary>
  [HttpPost("forgot-password")]
  [EnableRateLimiting("otp-policy")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
  {
    await _authService.ForgotPasswordAsync(request);
    return Ok(new { message = "Nếu email tồn tại, mã OTP đã được gửi." });
  }

  /// <summary>Verify the password reset OTP.</summary>
  [HttpPost("verify-reset-otp")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequestDto request)
  {
    var isValid = await _authService.VerifyResetOtpAsync(request);
    if (!isValid) return BadRequest(new { message = "Mã xác thực không chính xác hoặc đã hết hạn." });
    return Ok(new { message = "Mã xác thực hợp lệ." });
  }

  /// <summary>Reset password using the OTP code.</summary>
  [HttpPost("reset-password")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
  {
    try
    {
      await _authService.ResetPasswordAsync(request);
      return Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }

  /// <summary>Get current authenticated user info.</summary>
  [HttpGet("me")]
  [Authorize]
  [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public IActionResult Me()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
      ?? User.FindFirstValue("sub");

    var userInfo = new UserInfoDto
    {
      Id = Guid.Parse(userId!),
      Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
      FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
      AvatarUrl = User.FindFirstValue("avatar"),
      Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
    };

    return Ok(userInfo);
  }

  /// <summary>Update current user's profile.</summary>
  [HttpPut("profile")]
  [Authorize]
  [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
  {
    var userId = GetCurrentUserId();
    var result = await _authService.UpdateProfileAsync(userId, request);
    
    // Set refresh token as HttpOnly cookie if it was regenerated
    SetRefreshTokenCookie(result.RefreshToken);
    result.RefreshToken = string.Empty;

    return Ok(result);
  }

  /// <summary>Sets the refresh token as an HttpOnly cookie.</summary>
  private void SetRefreshTokenCookie(string token)
  {
    var cookieOptions = new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.Strict,
      Expires = DateTime.UtcNow.AddDays(7),
    };

    Response.Cookies.Append("refreshToken", token, cookieOptions);
  }

  private Guid GetCurrentUserId()
  {
    var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return sub != null ? Guid.Parse(sub) : throw new UnauthorizedAccessException();
  }
}
