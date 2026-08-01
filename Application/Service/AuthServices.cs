using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Application.Interface.Service;
using Gymora.Contracts.Authentication;
using Domain.Events;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Application.Model;
using Application.DTO.Model;

namespace Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailSender;
        private readonly CurrentUser _currentUser;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuthService(
            IAuthRepo authRepo,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailSender,
            CurrentUser currentUser,
            IPublishEndpoint publishEndpoint
            )
        {
            _authRepo = authRepo;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _currentUser = currentUser;
            _publishEndpoint = publishEndpoint;
        }



        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerReqDto, CancellationToken cancellationToken)
        {
            return await _authRepo.RegisterAsync(registerReqDto, cancellationToken);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginReqDto, CancellationToken cancellationToken)
        {
            return await _authRepo.LoginAsync(loginReqDto, cancellationToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string accessToken, CancellationToken cancellationToken)
        {
            return await _authRepo.RefreshTokenAsync(refreshToken, accessToken, cancellationToken);
        }

        public async Task LogoutAsync(int userId, string? refreshToken, bool logoutFromAllDevices, CancellationToken cancellationToken)
        {
            await _authRepo.LogoutAsync(userId, refreshToken, logoutFromAllDevices, cancellationToken);
        }

        public async Task<ConfirmEmailResponseDto> ConfirmEmailAsync(string email, string otp, CancellationToken cancellationToken)
        {
            await _authRepo.ConfirmEmailAsync(email, otp, cancellationToken);
            return new ConfirmEmailResponseDto { Success = true, Message = "Email confirmed successfully" };
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequest, CancellationToken cancellationToken)
        {
            var user = await _authRepo.GetUserByEmailAsync(forgotPasswordRequest.Email, cancellationToken);

            var successDto = new ForgotPasswordResponseDto
            {
                Message = "If an account with that email exists, a password reset code has been sent.",
                ExpirationInMinutes = 10
            };

            if (user == null || !user.IsActive)
            {
                return successDto;
            }

            if (user.PasswordResetOtpExpiry.HasValue &&
                user.PasswordResetOtpExpiry.Value.AddMinutes(-9) > DateTime.UtcNow)
            {
                throw new BadRequestException("Please wait 60 seconds before requesting another password reset code.");
            }

            var otp = await _authRepo.GeneratePasswordResetOtpAsync(forgotPasswordRequest.Email, cancellationToken);

            await _publishEndpoint.Publish(new SendPasswordResetCodeEvent(user.Email!, otp, 10), cancellationToken);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Password Reset OTP",
                $"Your OTP code is: <b>{otp}</b>. It will expire in 10 minutes."
            );

            return successDto;
        }

        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequestDto verifyOtpRequest, CancellationToken cancellationToken)
        {
            var resetToken = await _authRepo.VerifyOtpAsync(verifyOtpRequest.Email, verifyOtpRequest.Code, cancellationToken);

            return new VerifyOtpResponseDto
            {
                ResetToken = "",
                Message = "OTP verified successfully. You can now reset your password."
            };
        }

        public async Task<ResendOtpResponseDto> ResendOtpAsync(ResendOtpRequestDto dto, CancellationToken cancellationToken)
        {
            var user = await _authRepo.GetUserByEmailAsync(dto.Email, cancellationToken);

            var successDto = new ResendOtpResponseDto
            {
                Message = "If an account with that email exists, a password reset code has been sent.",
                ExpirationInMinutes = 10
            };

            if (user == null || !user.IsActive)
            {
                return successDto;
            }

            if (user.PasswordResetOtpExpiry.HasValue &&
                user.PasswordResetOtpExpiry.Value.AddMinutes(-9) > DateTime.UtcNow)
            {
                throw new BadRequestException("Please wait 60 seconds before requesting another password reset code.");
            }

            var otp = await _authRepo.GeneratePasswordResetOtpAsync(dto.Email, cancellationToken);

            await _publishEndpoint.Publish(new SendPasswordResetCodeEvent(user.Email!, otp, 10), cancellationToken);

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Password Reset OTP",
                $"Your OTP code is: <b>{otp}</b>. It will expire in 10 minutes."
            );

            return successDto;
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken)
        {
            var user = await _authRepo.GetUserByEmailAsync(resetPasswordRequest.Email, cancellationToken);
            if (user == null)
            {
                throw new BadRequestException("The password reset token is invalid, expired, or already used.");
            }

            await _authRepo.ResetPasswordAsync(user.Id, resetPasswordRequest.NewPassword, cancellationToken);

            await _publishEndpoint.Publish(new PasswordResetCompletedEvent(user.Id, user.Email!), cancellationToken);

            return new ResetPasswordResponseDto
            {
                Message = "Password has been reset successfully. You can now login with your new password."
            };
        }

        public async Task<GoogleAuthResponseDto> LoginWithGoogle(GoogleLoginRequestDto googleLoginRequest, CancellationToken cancellationToken)
        {
            return await _authRepo.LoginWithGoogle(googleLoginRequest, cancellationToken);
        }

        public async Task<GetUserProfileDto> GetUserProfileAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!_currentUser.IsAuthenticated) throw new UnauthorizedException("User is not authenticated.");

            return await _authRepo.GetUserProfileAsync(userId, cancellationToken);
        }

        public async Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
        {
            await _authRepo.ResendConfirmationEmailAsync(email, cancellationToken);
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!_currentUser.IsAuthenticated) throw new UnauthorizedException("User is not authenticated.");

            await _authRepo.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword, cancellationToken);
        }

        public async Task<AuthResponseDto> SwitchGym(SwitchGymRequest request, CancellationToken cancellationToken)
        {
            return await _authRepo.SwitchGym(request, cancellationToken);
        }
    }
}