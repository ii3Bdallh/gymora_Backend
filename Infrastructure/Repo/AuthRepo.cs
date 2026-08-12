using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Domain.Model.Base;
using Gymora.Contracts.Authentication;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Domain.Events;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Application.DTO.Model;

namespace Infrastructure.Repo
{
    public class AuthRepo : IAuthRepo
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtProvider _jwtProvider;
        private readonly ApplicationDbContext _context;

        private readonly int _refreshTokenDays;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IGymAccessRepo _gymAccessRepo;

        public AuthRepo(
            UserManager<ApplicationUser> userManager,
            JwtProvider jwtProvider,
            ApplicationDbContext context,
            IConfiguration configuration,
            IPublishEndpoint publishEndpoint,
            IGymAccessRepo gymAccessRepo,
            IUnitOfWork unitOfWork
            )
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _context = context;
            _publishEndpoint = publishEndpoint;
            _gymAccessRepo = gymAccessRepo;
            _unitOfWork = unitOfWork;
            _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenExpirationInDays"], out var days) ? days : 7;
        }



        #region Login / Register

        public async Task<ApplicationUser> RegisterAsync(RegisterRequestDto registerReqDto, CancellationToken cancellationToken)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerReqDto.Email);
            if (existingUser != null)
            {
                throw new ConflictException("This email address is already registered.");
            }

            var user = new ApplicationUser
            {
                UserName = registerReqDto.Email,
                PersonName = registerReqDto.PersonName.Trim(),
                Email = registerReqDto.Email,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerReqDto.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            await _userManager.AddToRoleAsync(user, AppRole.User);


            await _publishEndpoint.Publish(new UserRegisterdEvent(user.Id, user.Email), cancellationToken);
            return user;

        }


        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginReqDto, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
    .Include(x => x.RefreshTokens)
    .FirstOrDefaultAsync(x => x.Email == loginReqDto.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new LockoutException("Account is locked due to multiple failed login attempts.");
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Account is disabled. Please contact support.");
            }

            bool isValidPassword = await _userManager.CheckPasswordAsync(user, loginReqDto.Password);
            if (!isValidPassword)
            {
                await _userManager.AccessFailedAsync(user);
                throw new UnauthorizedException("Invalid email or password.");
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            var (plainRefreshToken, tokenHash) = _jwtProvider.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

            // Invalidate/Cleanup oldest sessions/devices if exceeding limits
            const int MaxActiveDevices = 5;
            var activeTokens = user.RefreshTokens.Where(rt => rt.IsValid).OrderBy(rt => rt.CreatedAt).ToList();
            if (activeTokens.Count >= MaxActiveDevices)
            {
                activeTokens.First().RevokedAt = DateTime.UtcNow;
            }




            var refresh = new RefreshToken
            {
                Token = tokenHash,
                ExpirationAt = refreshTokenExpiry,
                UserId = user.Id,
            };
            _context.RefreshTokens.Add(refresh);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, _) = _jwtProvider.GenerateToken(user, roles, refresh);

            // Publish Login Event
            await _publishEndpoint.Publish(new UserLoggedInEvent(user.Id, DateTime.UtcNow), cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = plainRefreshToken,
                User = new UserInfoDto
                {
                    UserId = user.Id.ToString(),
                    FullName = user.PersonName,
                    Email = user.Email!,
                    Role = roles
                },
            };
        }

        public async Task<GoogleAuthResponseDto> LoginWithGoogle(GoogleLoginRequestDto googleLoginRequest, CancellationToken cancellationToken)
        {
            var payload = await _jwtProvider.VerifyGoogleToken(googleLoginRequest.IdToken);
            if (payload == null)
            {
                throw new BadRequestException("The provided Google ID token is invalid or expired.");
            }

            var email = payload.Email;
            var user = await _userManager.Users
    .Include(x => x.RefreshTokens)
    .FirstOrDefaultAsync(x => x.Email == email);
            bool isNewUser = false;

            if (user == null)
            {
                isNewUser = true;
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PersonName = payload.Name,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }

                await _userManager.AddToRoleAsync(user, AppRole.User);




                await _publishEndpoint.Publish(new UserRegisteredViaGoogleEvent(user.Id, user.Email!, user.PersonName), cancellationToken);
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Account is disabled.");
            }
            var (plainRefreshToken, tokenHash) = _jwtProvider.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

            // Invalidate/Cleanup oldest sessions/devices if exceeding limits
            const int MaxActiveDevices = 5;
            var activeTokens = user.RefreshTokens.Where(rt => rt.IsValid).OrderBy(rt => rt.CreatedAt).ToList();
            if (activeTokens.Count >= MaxActiveDevices)
            {
                activeTokens.First().RevokedAt = DateTime.UtcNow;
            }
            var refresh = new RefreshToken
            {
                Token = tokenHash,
                ExpirationAt = refreshTokenExpiry,
                UserId = user.Id,
            };
            _context.RefreshTokens.Add(refresh);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);
            var (accessToken, _) = _jwtProvider.GenerateToken(user, roles, refresh);


            return new GoogleAuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = plainRefreshToken,
                IsNewUser = isNewUser,
                User = new UserInfoDto
                {
                    UserId = user.Id.ToString(),
                    FullName = user.PersonName,
                    Email = user.Email!
                },
            };
        }

        #endregion

        #region Refresh Token 
        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string accessToken, CancellationToken ct)
        {
            return await ProcessTokenRotationAsync(
                refreshToken,
                targetGymContext: null,
                ct);
        }

        public async Task<AuthResponseDto> SwitchGym(SwitchGymRequest switchGymRequest, CancellationToken ct)
        {
            // 1. جلب المستخدم للتحقق من صلاحية وصوله للجيم الجديد
            string? userIdStr = _jwtProvider.GetUserIdByToken(switchGymRequest.AccessToken, validateLifetime: false);
            if (userIdStr == null) throw new UnauthorizedException("Invalid access token.");

            int userId = int.Parse(userIdStr);

            // 2. التحقق من صلاحيات الوصول للفرع/الجيم الجديد
            MyGymDto? gymAccess = await _gymAccessRepo.GetGymAccessAsync(userId, switchGymRequest.GymId, ct);
            if (gymAccess == null) throw new UnauthorizedException("You don't have access to this gym.");

            // 3. تجهيز السياق الجديد للـ Gym
            var newGymContext = new MyGymDto
            {
                GymId = gymAccess.GymId,
                GymPeopleId = gymAccess.GymPeopleId,
                GymRole = gymAccess.GymRole,
                GymName = gymAccess.GymName
            };

            // 4. تنفيذ تجديد التوكن وتطبيق السياق الجديد
            return await ProcessTokenRotationAsync(switchGymRequest.RefreshToken, newGymContext, ct);
        }

        private async Task<AuthResponseDto> ProcessTokenRotationAsync(
    string refreshToken,
    MyGymDto? targetGymContext,
    CancellationToken ct)
        {



            var tokenHash = _jwtProvider.HashToken(refreshToken);
            RefreshToken? existingRefreshToken = await _context.RefreshTokens
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                    x.Token == tokenHash &&
                    x.RevokedAt == null,
                    ct);

            if (existingRefreshToken == null)
                throw new UnauthorizedException("Invalid refresh token.");

            if (!existingRefreshToken.IsValid)
                throw new UnauthorizedException("Refresh token expired or revoked.");

            ApplicationUser? user = existingRefreshToken.User;

            if (user == null) throw new BadRequestException("User not found.");




            // Revoke current refresh token (Rotation)
            existingRefreshToken.RevokedAt = DateTime.UtcNow;

            // Determine Target Gym Context
            int currentGymId = targetGymContext?.GymId ?? existingRefreshToken.CurrentGymId;
            int currentGymPeopleId = targetGymContext?.GymPeopleId ?? existingRefreshToken.CurrentGymPeopleId ?? 0;
            string? gymRole = targetGymContext?.GymRole ?? existingRefreshToken.GymRole;

            // Create new Refresh Token
            var (newPlainRefreshToken, newHash) = _jwtProvider.GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

            var newRefresh = new RefreshToken
            {
                Token = newHash,
                ExpirationAt = newExpiry,
                UserId = user.Id,
                CurrentGymId = currentGymId,
                CurrentGymPeopleId = currentGymPeopleId,
                GymRole = gymRole
            };

            _context.RefreshTokens.Add(newRefresh);
            await _unitOfWork.SaveChangesAsync(ct);

            // Generate New Access Token
            var roles = await _userManager.GetRolesAsync(user);
            var (newAccessToken, _) = _jwtProvider.GenerateToken(user, roles, newRefresh);



            CurrentGymDto? currentGym = null;
            if (currentGymId > 0 && !string.IsNullOrEmpty(gymRole))
            {
                currentGym = new CurrentGymDto
                {
                    GymId = currentGymId.ToString(),
                    Role = gymRole
                };
            }

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newPlainRefreshToken,
                User = new UserInfoDto
                {
                    UserId = user.Id.ToString(),
                    FullName = user.PersonName,
                    Email = user.Email!
                },
                CurrentGym = currentGym
            };
        }
        #endregion

        #region Otp





        public async Task<string> GenerateEmailConfirmationOtpAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            string otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.EmailConfirmationOtp = HashOtp(otp);
            user.EmailConfirmationOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.EmailConfirmationOtpAttempts = 0;
            await _userManager.UpdateAsync(user);
            return otp;
        }

        public async Task VerifyEmailConfirmationOtpAsync(string email, string otp, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new NotFoundException("User not found.");

            if (user.EmailConfirmed) throw new BadRequestException("Email already confirmed.");

            if (string.IsNullOrEmpty(user.EmailConfirmationOtp) || user.EmailConfirmationOtpExpiry < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired code.");

            if (user.EmailConfirmationOtpAttempts >= 5)
            {
                user.EmailConfirmationOtp = null;
                user.EmailConfirmationOtpExpiry = null;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Too many attempts. Please request a new code.");
            }

            var hashedOtp = HashOtp(otp);
            if (user.EmailConfirmationOtp != hashedOtp)
            {
                user.EmailConfirmationOtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Invalid or expired code.");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationOtp = null;
            user.EmailConfirmationOtpExpiry = null;
            user.EmailConfirmationOtpAttempts = 0;
            await _userManager.UpdateAsync(user);
        }
        public async Task<string> GeneratePasswordResetOtpAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            string otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            user.PasswordResetOtp = HashOtp(otp);
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return otp;
        }


        public async Task VerifyPasswordResetOtpAsync(string email, string otp, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new NotFoundException("User not found.");

            if (string.IsNullOrEmpty(user.PasswordResetOtp) || user.PasswordResetOtpExpiry < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired code.");

            if (user.PasswordResetOtpAttempts >= 5)
            {
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Too many attempts. Please request a new code.");
            }

            var hashedOtp = HashOtp(otp);
            if (user.PasswordResetOtp != hashedOtp)
            {
                user.PasswordResetOtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Invalid or expired code.");
            }

            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);
        }


        #endregion

        #region Password
        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            foreach (var rt in user.RefreshTokens.Where(x => x.IsValid))
            {
                rt.RevokedAt = DateTime.UtcNow;
            }
            await _userManager.UpdateAsync(user);
        }



        public async Task ResetPasswordAsync(int userId, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
            await _userManager.UpdateAsync(user);
        }

        #endregion

        public async Task LogoutAsync(int userId, string? refreshToken, bool logoutFromAllDevices, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null) return;

            if (logoutFromAllDevices)
            {
                foreach (var rt in user.RefreshTokens.Where(x => x.IsValid))
                {
                    rt.RevokedAt = DateTime.UtcNow;
                }
            }
            else if (!string.IsNullOrEmpty(refreshToken))
            {
                var tokenHash = _jwtProvider.HashToken(refreshToken);
                var rt = user.RefreshTokens.FirstOrDefault(x => x.Token == tokenHash);
                if (rt != null)
                {
                    rt.RevokedAt = DateTime.UtcNow;
                }
            }

            await _userManager.UpdateAsync(user);
        }



        public async Task<GetUserProfileDto> GetUserProfileAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new NotFoundException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            return new GetUserProfileDto(
                user.Email!,
                user.PersonName,
                user.PhoneNumber,
                roles
            );
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _userManager.FindByEmailAsync(email);
        }


        #region Helper

        private static string HashOtp(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }






        #endregion



    }
}
