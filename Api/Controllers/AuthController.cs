using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.StaticTexts;
using Gymora.Contracts.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGymService _gymService;



        public AuthController(IAuthService authService, IGymService gymService)
        {
            _authService = authService;
            _gymService = gymService;
        }



        [Authorize]
        [HttpPost("switch")]
        public async Task<IActionResult> SwitchGym([FromBody] SwitchGymRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<AuthResponseDto>.Failure("VALIDATION_ERROR", "Invalid request parameters."));

            var res = await _authService.SwitchGym(request, ct);
            return Ok(Result<AuthResponseDto>.Success(res));
        }

        [Authorize]
        [HttpGet("get-user-profile")]
        public async Task<IActionResult> GetUserProfile(CancellationToken cancellationToken)
        {
            var result = await _authService.GetUserProfileAsync(cancellationToken);
            return Ok(Result<GetUserProfileDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ConfirmEmailResponseDto>.Failure("VALIDATION_ERROR", "Invalid request parameters."));

            var result = await _authService.ConfirmEmailAsync(dto.Email, dto.Otp, cancellationToken);
            return Ok(Result<ConfirmEmailResponseDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("resend-confirmation-email")]
        [EnableRateLimiting("Ip_3Limit_5Min")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<string>.Failure("VALIDATION_ERROR", "Invalid request parameters."));

            await _authService.ResendConfirmationEmailAsync(dto.Email, cancellationToken);
            return Ok(Result<string>.Success("If the email exists, a confirmation code has been sent."));
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerReqDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<RegisterResponseDto>.Failure("VALIDATION_ERROR", "Invalid input data or weak password format."));

            var result = await _authService.RegisterAsync(registerReqDto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, Result<RegisterResponseDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("Ip_5Limit_1Min")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginReqDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<object>.Failure("VALIDATION_ERROR", "Invalid input data."));

            var result = await _authService.LoginAsync(loginReqDto, cancellationToken);



            return Ok(Result<AuthResponseDto>.Success(result));
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReqDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<AuthResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, dto.AccessToken, cancellationToken);
            return Ok(Result<AuthResponseDto>.Success(result));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest dto, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
                dto.UserId = int.Parse(userIdClaim);

            await _authService.LogoutAsync(dto.UserId, dto.RefreshToken, dto.LogoutFromAllDevices, cancellationToken);

            var message = dto.LogoutFromAllDevices
                ? "Successfully logged out from all devices"
                : "Successfully logged out";

            return Ok(Result<string>.Success(message));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [EnableRateLimiting("Ip_3Limit_5Min")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ForgotPasswordResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.ForgotPasswordAsync(dto, cancellationToken);
            return Ok(Result<ForgotPasswordResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        [EnableRateLimiting("Ip_10Limit_1Min")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<VerifyOtpResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.VerifyOtpAsync(dto, cancellationToken);
            return Ok(Result<VerifyOtpResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("resend-otp")]
        [EnableRateLimiting("Ip_3Limit_5Min")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ResendOtpResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.ResendOtpAsync(dto, cancellationToken);
            return Ok(Result<ResendOtpResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        [EnableRateLimiting("Ip_10Limit_1Min")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ResetPasswordResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.ResetPasswordAsync(dto, cancellationToken);
            return Ok(Result<ResetPasswordResponseDto>.Success(res));
        }




        [AllowAnonymous]
        [HttpGet("confirm-email")]
        [EnableRateLimiting("Ip_10Limit_1Min")]
        public async Task<IActionResult> ConfirmEmailPage(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return Content(GenerateConfirmEmailPage(false), "text/html");

            try
            {
                await _authService.ConfirmEmailAsync(userId, code, HttpContext.RequestAborted);
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
                return UnprocessableEntity(Result<string>.Failure("VALIDATION_ERROR", "Invalid input."));

            await _authService.ChangePasswordAsync(dto, cancellationToken);
            return Ok(Result<string>.Success("Password changed successfully. Please login again on other devices."));
        }

        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<GoogleAuthResponseDto>.Failure("VALIDATION_ERROR", "Invalid request parameters."));

            var result = await _authService.LoginWithGoogle(dto, HttpContext.RequestAborted);
            return Ok(Result<GoogleAuthResponseDto>.Success(result));
        }
    }
}
