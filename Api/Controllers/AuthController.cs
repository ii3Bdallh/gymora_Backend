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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerReqDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<string>.Failure("VALIDATION_ERROR", "Invalid input data or weak password format."));

            await _authService.RegisterAsync(registerReqDto, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, Result<string>.Success("A confirmation code has been sent."));
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
        [HttpPost("verify-email-otp")]
        [EnableRateLimiting("Ip_10Limit_1Min")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<string>.Failure("VALIDATION_ERROR", "Invalid input."));

            await _authService.VerifyEmailOtpAsync(dto, cancellationToken);
            return Ok(Result<string>.Success("Email verified successfully."));
        }

        [AllowAnonymous]
        [HttpPost("resend-email-otp")]
        [EnableRateLimiting("Ip_3Limit_5Min")]
        public async Task<IActionResult> ResendEmailOtp([FromBody] ResendOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ResendOtpResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.ResendEmailOtpAsync(dto, cancellationToken);
            return Ok(Result<ResendOtpResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("verify-password-otp")]
        [EnableRateLimiting("Ip_10Limit_1Min")]
        public async Task<IActionResult> VerifyPasswordOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<VerifyOtpResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.VerifyPasswordOtpAsync(dto, cancellationToken);
            return Ok(Result<VerifyOtpResponseDto>.Success(res));
        }

        [AllowAnonymous]
        [HttpPost("resend-password-otp")]
        [EnableRateLimiting("Ip_3Limit_5Min")]
        public async Task<IActionResult> ResendPasswordOtp([FromBody] ResendOtpRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return UnprocessableEntity(Result<ResendOtpResponseDto>.Failure("VALIDATION_ERROR", "Invalid input."));

            var res = await _authService.ResendPasswordOtpAsync(dto, cancellationToken);
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
