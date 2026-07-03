using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Request;
using Application.DTO.Response;
using Application.Interface.Repo;
using Application.Interface.Service;
using Application.Interface.Service.Shared;
using Domain.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class AuthService(IAuthRepo authRepo, IHttpContextAccessor httpContextAccessor, IEmailService emailSender) : IAuthService
    {
        public async Task RegisterAsync(RegisterReqDto registerReqDto, CancellationToken cancellationToken)
        {
            await authRepo.RegisterAsync(registerReqDto, cancellationToken);

            var user = await authRepo.GetUserByEmailAsync(registerReqDto.Email, cancellationToken);
            if (user is not null && !string.IsNullOrEmpty(user.Email))
            {
                var otp = await authRepo.GenerateEmailConfirmationOtpAsync(user, cancellationToken);
                await emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Your confirmation code is: <b>{otp}</b>. It expires in 10 minutes.");
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest dto, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("No HTTP context available.");

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User is not authenticated.");

            await authRepo.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword, cancellationToken);
        }

        public async Task<ConfirmEmailResponseDto> ConfirmEmailAsync(string email, string otp, CancellationToken cancellationToken)
        {
            await authRepo.ConfirmEmailAsync(email, otp, cancellationToken);
            return new ConfirmEmailResponseDto { Success = true, Message = "Email confirmed successfully" };
        }

        public async Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
        {
            await authRepo.ResendConfirmationEmailAsync(email, cancellationToken);
        }
        public async Task<LoginResDto> LoginAsync(LoginReqDto loginReqDto, CancellationToken cancellationToken)
        {
            return await authRepo.LoginAsync(loginReqDto, cancellationToken);
        }

        public async Task<LoginResDto> RefreshTokenAsync(RefreshTokenReqDto refreshTokenReqDto, CancellationToken cancellationToken)
        {
            var user = await authRepo.RefreshTokenAsync(refreshTokenReqDto, cancellationToken);
            return user;
        }



        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest, CancellationToken cancellationToken)
        {
            // Generate OTP in repo
            var otp = await authRepo.GeneratePasswordResetOtpAsync(forgotPasswordRequest.Email, cancellationToken);

            // Send OTP email
            await emailSender.SendEmailAsync(
                forgotPasswordRequest.Email,
                "Password Reset OTP",
                $"Your OTP code is: <b>{otp}</b>. It will expire in 5 minutes."
            );

            return new ForgotPasswordResponseDto
            {
                Success = true,
                Message = "Password reset OTP sent to your email"
            };
        }

        public async Task<VerifyOtpResponseDto> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest, CancellationToken cancellationToken)
        {
            // Repo throws exception if invalid, so if we reach here, it's valid
            await authRepo.VerifyOtpAsync(verifyOtpRequest, cancellationToken);

            return new VerifyOtpResponseDto
            {
                IsValid = true,
                Message = "OTP verified successfully"
            };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
        {
            // Verify OTP first (business logic in service)
            await authRepo.VerifyOtpAsync(
                new VerifyOtpRequest(resetPasswordRequest.Email, resetPasswordRequest.Otp),
                cancellationToken
            );

            // Reset password in repo
            await authRepo.ResetPasswordAsync(resetPasswordRequest, cancellationToken);

            return new ResetPasswordResponseDto
            {
                Success = true,
                Message = "Password reset successfully. You can now log in with your new password."
            };
        }

        public async Task<LoginResDto> LoginWithGoogle(GoogleLoginRequest googleLoginRequest, CancellationToken cancellationToken)
        {
            LoginResDto user = await authRepo.LoginWithGoogle(googleLoginRequest, cancellationToken);
            return user;
        }

        public Task<GetUserProfileDto> GetUserProfileAsync(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new UnauthorizedAccessException("No HTTP context available.");
            }

            var user = httpContext.User;
            if (user == null)
            {
                throw new UnauthorizedAccessException("User is not available in HTTP context.");
            }

            var nameIdentifierClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim == null || string.IsNullOrEmpty(nameIdentifierClaim.Value))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            int userId;
            if (!int.TryParse(nameIdentifierClaim.Value, out userId) || userId == 0)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return authRepo.GetUserProfileAsync(userId, cancellationToken);
        }

        public async Task LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken)
        {
            await authRepo.LogoutAsync(logoutRequest, cancellationToken);
        }
    }
}

