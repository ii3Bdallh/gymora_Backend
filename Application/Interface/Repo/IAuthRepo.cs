using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Request;
using Application.DTO.Response;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IAuthRepo
    {
        public Task RegisterAsync(RegisterReqDto registerReqDto, CancellationToken cancellationToken);

        public Task<LoginResDto> LoginAsync(LoginReqDto loginReqDto, CancellationToken cancellationToken);

        public Task<LoginResDto> RefreshTokenAsync(RefreshTokenReqDto refreshTokenReqDto, CancellationToken cancellationToken);

        public Task LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken);


        public Task<bool> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest, CancellationToken cancellationToken);

        public Task<string> GeneratePasswordResetOtpAsync(string email, CancellationToken cancellationToken);

        public Task ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken);

        public Task<LoginResDto> LoginWithGoogle(GoogleLoginRequest googleLoginRequest, CancellationToken cancellationToken);

        public Task<GetUserProfileDto> GetUserProfileAsync(int userId, CancellationToken cancellationToken);

        public Task<AppUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);

        public Task<string> GenerateEmailConfirmationTokenAsync(AppUser user, CancellationToken cancellationToken = default);

        public Task<string> GenerateEmailConfirmationOtpAsync(AppUser user, CancellationToken cancellationToken = default);
        public Task ConfirmEmailAsync(string email, string otp, CancellationToken cancellationToken);
        public Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken);

        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken);
    }
}



