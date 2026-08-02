using Gymora.Contracts.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Auth;
using Application.DTO.Model;

namespace Application.Interface.Service
{
    public interface IAuthService
    {
        Task ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken cancellationToken);

        public Task RegisterAsync(RegisterRequestDto registerReqDto, CancellationToken cancellationToken);

        public Task<AuthResponseDto> LoginAsync(LoginRequestDto loginReqDto, CancellationToken cancellationToken);

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken);

        public Task LogoutAsync(int userId, string? refreshToken, bool logoutFromAllDevices, CancellationToken cancellationToken);

        public Task VerifyEmailOtpAsync(VerifyOtpRequestDto verifyOtpRequest, CancellationToken cancellationToken);

        public Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest, CancellationToken cancellationToken);

        public Task<VerifyOtpResponseDto> VerifyPasswordOtpAsync(VerifyOtpRequestDto verifyOtpRequest, CancellationToken cancellationToken);

        public Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken);

        public Task<GoogleAuthResponseDto> LoginWithGoogle(GoogleLoginRequestDto googleLoginRequest, CancellationToken cancellationToken);

        public Task<GetUserProfileDto> GetUserProfileAsync(CancellationToken cancellationToken);

        public Task<ResendOtpResponseDto> ResendEmailOtpAsync(ResendOtpRequestDto dto, CancellationToken cancellationToken);

        public Task<ResendOtpResponseDto> ResendPasswordOtpAsync(ResendOtpRequestDto dto, CancellationToken cancellationToken);

        public Task<AuthResponseDto> SwitchGym(SwitchGymRequest request, CancellationToken cancellationToken);

    }
}
