using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interface.Service;
using Application.StaticTexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [Authorize]
        [HttpGet("get-user-profile")]
        public async Task<IActionResult> GetUserProfile(CancellationToken cancellationToken)
        {
            var result = await authService.GetUserProfileAsync(cancellationToken);
            return Ok(Result<GetUserProfileDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            var result = await authService.ConfirmEmailAsync(dto.Email, dto.Otp, cancellationToken);
            return Ok(Result<ConfirmEmailResponseDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("resend-confirmation-email")]
        [EnableRateLimiting("otp-sensitive")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            await authService.ResendConfirmationEmailAsync(dto.Email, cancellationToken);
            return Ok(Result<string>.Success("If the email exists, a confirmation code has been sent."));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterReqDto registerReqDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            await authService.RegisterAsync(registerReqDto, HttpContext.RequestAborted);
            return Ok(Result<string>.Success("User registered successfully. Please check your email to confirm your account."));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDto loginReqDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            var result = await authService.LoginAsync(loginReqDto, cancellationToken);
            return Ok(Result<LoginResDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReqDto refreshTokenReqDto, CancellationToken cancellationToken)
        {
            var result = await authService.RefreshTokenAsync(refreshTokenReqDto, cancellationToken);
            return Ok(Result<LoginResDto>.Success(result));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest dto, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
                dto = dto with { UserId = int.Parse(userIdClaim) };

            await authService.LogoutAsync(dto, cancellationToken);

            var message = dto.LogoutFromAllDevices
                ? "Successfully logged out from all devices"
                : "Successfully logged out";

            return Ok(Result<string>.Success(message));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [EnableRateLimiting("otp-sensitive")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto, CancellationToken cancellationToken)
        {
            var res = await authService.ForgotPasswordAsync(dto, cancellationToken);
            return Ok(Result<ForgotPasswordResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        [EnableRateLimiting("otp-verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest dto, CancellationToken cancellationToken)
        {
            var res = await authService.VerifyOtpAsync(dto, cancellationToken);
            return Ok(Result<VerifyOtpResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        [EnableRateLimiting("otp-verify")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            var res = await authService.ResetPasswordAsync(dto, cancellationToken);
            return Ok(Result<ResetPasswordResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpGet("confirm-email")]
        [EnableRateLimiting("otp-verify")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return Content(GenerateConfirmEmailPage(false), "text/html");

            try
            {
                await authService.ConfirmEmailAsync(userId, code, HttpContext.RequestAborted);
                return Content(GenerateConfirmEmailPage(true), "text/html");
            }
            catch
            {
                return Content(GenerateConfirmEmailPage(false), "text/html");
            }
        }

        private static string GenerateConfirmEmailPage(bool isSuccess)
        {
            var title = isSuccess ? ConfirmEmailPageTexts.SuccessTitle : ConfirmEmailPageTexts.ErrorTitle;
            var message = isSuccess ? ConfirmEmailPageTexts.SuccessMessage : ConfirmEmailPageTexts.ErrorMessage;
            var icon = isSuccess ? "&#10003;" : "&#10007;";
            var iconClass = isSuccess ? "success" : "error";

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{ConfirmEmailPageTexts.PageTitle}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; display: flex; justify-content: center; align-items: center; min-height: 100vh; }}
        .card {{ background: white; border-radius: 12px; padding: 48px; text-align: center; box-shadow: 0 4px 24px rgba(0,0,0,0.1); max-width: 440px; width: 90%; }}
        .icon {{ width: 72px; height: 72px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; font-size: 36px; }}
        .icon.success {{ background: #e8f5e9; color: #2e7d32; }}
        .icon.error {{ background: #fbe9e7; color: #c62828; }}
        h1 {{ font-size: 24px; margin-bottom: 12px; color: #1a1a1a; }}
        p {{ color: #666; line-height: 1.6; }}
        .footer {{ margin-top: 32px; font-size: 12px; color: #999; }}
    </style>
</head>
<body>
    <div class=""card"">
        <div class=""icon {iconClass}"">{icon}</div>
        <h1>{title}</h1>
        <p>{message}</p>
        <div class=""footer"">{ConfirmEmailPageTexts.FooterText}</div>
    </div>
</body>
</html>";
        }
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(Result<string>.Failure("VALIDATION_ERROR", "Invalid input"));

            await authService.ChangePasswordAsync(dto, cancellationToken);
            return Ok(Result<string>.Success("Password changed successfully. Please login again on other devices."));
        }
        [AllowAnonymous]
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequest dto)
        {
            var result = await authService.LoginWithGoogle(dto, HttpContext.RequestAborted);
            return Ok(Result<LoginResDto>.Success(result));
        }
    }
}
