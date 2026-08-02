using Gymora.Contracts.Authentication;
using Domain.Model.Auth;
using System.Threading.Tasks;
using System.Threading;
using Application.DTO.Auth;
using Application.DTO.Model;

namespace Application.Interface.Repo.Shared
{
    public interface IAuthRepo
    {
        public Task<ApplicationUser> RegisterAsync(RegisterRequestDto registerReqDto, CancellationToken cancellationToken);

        public Task<AuthResponseDto> LoginAsync(LoginRequestDto loginReqDto, CancellationToken cancellationToken);

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken);

        public Task LogoutAsync(int userId, string? refreshToken, bool logoutFromAllDevices, CancellationToken cancellationToken);

        public Task<GoogleAuthResponseDto> LoginWithGoogle(GoogleLoginRequestDto googleLoginRequest, CancellationToken cancellationToken);

        public Task<GetUserProfileDto> GetUserProfileAsync(int userId, CancellationToken cancellationToken);

        public Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);


        #region Otp
        public Task<string> GenerateEmailConfirmationOtpAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        public Task VerifyEmailConfirmationOtpAsync(string email, string otp, CancellationToken cancellationToken);

        public Task<string> GeneratePasswordResetOtpAsync(ApplicationUser user, CancellationToken cancellationToken);


        public Task VerifyPasswordResetOtpAsync(string email, string otp, CancellationToken cancellationToken);


        #endregion

        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken);


        Task ResetPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken);


        Task<AuthResponseDto> SwitchGym(SwitchGymRequest switchGymRequest, CancellationToken ct);
    }
}
