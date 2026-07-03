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

namespace Application.Interface.Service
{
    public interface IAuthService
    {
        Task ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken cancellationToken);

        public Task RegisterAsync(RegisterReqDto registerReqDto, CancellationToken cancellationToken);

        public Task<LoginResDto> LoginAsync(LoginReqDto loginReqDto, CancellationToken cancellationToken);

        public Task<LoginResDto> RefreshTokenAsync(RefreshTokenReqDto refreshTokenReqDto, CancellationToken cancellationToken);

        public Task LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken);

        public Task<ConfirmEmailResponseDto> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken);

        public Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest, CancellationToken cancellationToken);

        public Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest, CancellationToken cancellationToken);

        public Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken);

        public Task<LoginResDto> LoginWithGoogle(GoogleLoginRequest googleLoginRequest, CancellationToken cancellationToken);

        public Task<GetUserProfileDto> GetUserProfileAsync(CancellationToken cancellationToken);

        public Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken);
    }
}



