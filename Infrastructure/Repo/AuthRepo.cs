using Application.DTO;
using Application.DTO.Auth;
using Application.DTO.Exceptions;
using Application.DTO.Model;
using Application.Interface.Repo;
using Application.Interface.Repo.Shared;
using Application.Interface.Service.Shared;
using Domain.Enum;
using Domain.Model;
using Domain.Model.Auth;
using Infrastructure.Persistence;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Infrastructure.Repo
{
    public class AuthRepo : IAuthRepo
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtProvider _jwtProvider;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthRepo> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _refreshTokenDays;

        private readonly IGymAccessRepo _gymAccessRepo;

        public AuthRepo(
            UserManager<ApplicationUser> userManager,
            JwtProvider jwtProvider,
            ApplicationDbContext context,
            IEmailService emailSender,
            IConfiguration configuration,
            ILogger<AuthRepo> logger,
            IUnitOfWork unitOfWork,
            IGymAccessRepo gymAccessRepo
            )
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _context = context;
            _emailSender = emailSender;
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenExpirationInDays"], out var days) ? days : 7;
            _gymAccessRepo = gymAccessRepo;
        }

        public async Task<GetUserProfileDto> GetUserProfileAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new NotFoundException("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return new GetUserProfileDto(
                Email: user.Email!,
                PersonName: user.PersonName,
                Roles: roles,
                PhoneNumber: user.PhoneNumber
            );
        }

        public async Task RegisterAsync(RegisterReqDto registerReqDto, CancellationToken cancellationToken)
        {
            ApplicationUser user = new ApplicationUser
            {
                UserName = registerReqDto.Email,
                PersonName = registerReqDto.UserName,
                Email = registerReqDto.Email,
                PhoneNumber = registerReqDto.PhoneNumber,
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerReqDto.Password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var defaultRole = RoleType.User.ToString();
            await _userManager.AddToRoleAsync(user, defaultRole);
        }

        public async Task<LoginResDto> LoginAsync(LoginReqDto loginReqDto, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(loginReqDto.Email);
            if (user is null)
                throw new BadRequestException("Invalid email or password");

            if (await _userManager.IsLockedOutAsync(user))
                throw new BadRequestException("Account is locked. Please try again later.");

            bool isEmailVerified = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailVerified)
            {
                var otp = await GenerateEmailConfirmationOtpAsync(user, cancellationToken);
                await _emailSender.SendEmailAsync(user.Email!, "Confirm your email",
                    $"Your confirmation code is: <b>{otp}</b>. It expires in 10 minutes.");
                throw new BadRequestException("Email is not confirmed. A new code has been sent.");
            }

            bool isValidPassword = await _userManager.CheckPasswordAsync(user, loginReqDto.Password);
            if (!isValidPassword)
            {
                await _userManager.AccessFailedAsync(user);
                throw new BadRequestException("Invalid email or password");
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);



            try
            {
                // ---- حد أقصى لعدد الأجهزة المسجلة (Refresh Tokens) ----
                const int MaxActiveDevices = 5;
                var activeTokens = user.RefreshTokens.Where(rt => rt.IsValid).OrderBy(rt => rt.CreatedAt).ToList();
                if (activeTokens.Count >= MaxActiveDevices)
                    activeTokens.First().RevokedAt = DateTime.UtcNow;

                string refreshToken = GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);

                RefreshToken refresh = new RefreshToken
                {
                    Token = refreshToken,
                    ExpirationAt = refreshTokenExpiry,
                };

                user.RefreshTokens.Add(refresh);


                var (token, expiresIn) = _jwtProvider.GenerateToken(
     user,
     roles,
     refresh
 );
                await _userManager.UpdateAsync(user);

                return new LoginResDto(user.Id, user.Email!, user.PersonName, token, expiresIn,
                    refreshToken, roles, refreshTokenExpiry, null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login transaction failed for user {Email}", loginReqDto.Email);
                throw;
            }
        }
        public async Task<LoginResDto> RefreshTokenAsync(RefreshTokenReqDto refreshTokenReqDto, CancellationToken cancellationToken)
        {
            string refreshToken = refreshTokenReqDto.RefreshToken;
            string accessToken = refreshTokenReqDto.AccessToken;

            string? userId = _jwtProvider.GetUserIdByToken(accessToken, validateLifetime: false);

            if (userId is null)
                throw new UnauthorizedException("Invalid access token");

            ApplicationUser? user = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId), cancellationToken);

            if (user is null)
                throw new BadRequestException("User not found");

            RefreshToken? existingRefreshToken = user.RefreshTokens
                .FirstOrDefault(rt => rt.Token == refreshToken);
            if (existingRefreshToken is null || !existingRefreshToken.IsValid)
            {
                if (existingRefreshToken is not null && existingRefreshToken.RevokedAt.HasValue)
                {
                    _logger.LogWarning("Refresh token reuse detected for user {UserId}. Revoking all tokens.", userId);
                    foreach (var token in user.RefreshTokens.Where(rt => rt.IsValid))
                    {
                        token.RevokedAt = DateTime.UtcNow;
                    }
                    await _userManager.UpdateAsync(user);
                }
                throw new UnauthorizedException("Invalid refresh token");
            }

            existingRefreshToken.RevokedAt = DateTime.UtcNow;

            string newRefreshToken = GenerateRefreshToken();
            DateTime refreshTokenExpirationDate = DateTime.UtcNow.AddDays(_refreshTokenDays);
            var currentGym = await _gymAccessRepo.GetGymAccessAsync(user.Id, existingRefreshToken.CurrentGymId,
              cancellationToken);

            


            RefreshToken newRefreshTokenRecord = new RefreshToken
            {
                Token = newRefreshToken,
                ExpirationAt = refreshTokenExpirationDate,
                CreatedAt = DateTime.UtcNow,
                CurrentGymId = currentGym?.GymId ?? 0,
            };
            user.RefreshTokens.Add(newRefreshTokenRecord);


            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            (string newAccessToken, int expiresIn) = _jwtProvider.GenerateToken(user, roles, newRefreshTokenRecord);

            return new LoginResDto(
                Id: user.Id,
                Email: user.Email!,
                PersonName: user.PersonName,
                Token: newAccessToken,
                ExpiresIn: expiresIn,
                Refreshtoken: newRefreshToken,
                Roles: roles,
                RefreshTokenExpirationDate: refreshTokenExpirationDate,
                MyGym: new MyGymDto
                {
                    GymId = currentGym?.GymId ?? 0,
                    GymName = currentGym?.GymName ?? string.Empty,
                    GymRole = currentGym?.GymRole ?? string.Empty,
                }
            );
        }

        #region ConfirmEmailAsync
        public async Task ConfirmEmailAsync(string email, string otp, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                throw new NotFoundException("User not found");

            if (user.EmailConfirmed)
                throw new BadRequestException("Email already confirmed");

            if (string.IsNullOrEmpty(user.EmailConfirmationOtp) || user.EmailConfirmationOtpExpiry < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired code");

            if (user.EmailConfirmationOtpAttempts >= 5)
            {
                user.EmailConfirmationOtp = null;
                user.EmailConfirmationOtpExpiry = null;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Too many attempts. Please request a new code.");
            }

            if (!VerifyOtpHash(otp, user.EmailConfirmationOtp))
            {
                user.EmailConfirmationOtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Invalid or expired code");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationOtp = null;
            user.EmailConfirmationOtpExpiry = null;
            user.EmailConfirmationOtpAttempts = 0;
            await _userManager.UpdateAsync(user);
        }

        public async Task ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || user.EmailConfirmed)
                return; // ما بنكشفش لو الإيميل موجود ولا لأ

            var otp = await GenerateEmailConfirmationOtpAsync(user, cancellationToken);
            await _emailSender.SendEmailAsync(user.Email!, "Confirm your email",
                $"Your confirmation code is: <b>{otp}</b>. It expires in 10 minutes.");
        }
        #endregion

        #region Logout
        public async Task LogoutAsync(LogoutRequest logoutRequest, CancellationToken cancellationToken)
        {
            if (logoutRequest.LogoutFromAllDevices)
            {
                var userRefreshTokens = await _context.RefreshTokens
                    .Where(rf => rf.UserId == logoutRequest.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var token in userRefreshTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == logoutRequest.RefreshToken, cancellationToken);

                if (refreshToken != null)
                {
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
        }
        #endregion

        #region ChangePasswordAsync
        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
                throw new NotFoundException("User not found");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            // Best practice: امسح كل الـ refresh tokens بعد تغيير الباسورد (يقفل كل الأجهزة عدا الحالي)
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var userWithTokens = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstAsync(u => u.Id == userId, cancellationToken);

                foreach (var token in userWithTokens.RefreshTokens.Where(rt => rt.IsValid))
                    token.RevokedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(userWithTokens);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke tokens after password change for user {UserId}", userId);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        #endregion

        #region GeneratePasswordResetOtpAsync
        public async Task<string> GeneratePasswordResetOtpAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
                return "00000";
            }

            string otp = RandomNumberGenerator.GetInt32(10000, 99999).ToString();

            user.PasswordResetOtp = HashOtp(otp);
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);
            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);

            return otp;
        }

        public async Task<bool> VerifyOtpAsync(VerifyOtpRequest verifyOtpRequest, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(verifyOtpRequest.Email);
            if (user is null) throw new NotFoundException("User not found");

            if (string.IsNullOrEmpty(user.PasswordResetOtp) || user.PasswordResetOtpExpiry < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired OTP");

            if (user.PasswordResetOtpAttempts >= 5)
            {
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Too many attempts. Please request a new OTP.");
            }

            if (!VerifyOtpHash(verifyOtpRequest.Otp, user.PasswordResetOtp))
            {
                user.PasswordResetOtpAttempts++;
                await _userManager.UpdateAsync(user);
                throw new BadRequestException("Invalid or expired OTP");
            }

            user.PasswordResetOtpAttempts = 0;
            await _userManager.UpdateAsync(user);
            return true;
        }
        public async Task ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
            if (user is null) throw new NotFoundException("User not found");

            bool isSamePassword = await _userManager.CheckPasswordAsync(user, resetPasswordRequest.NewPassword);
            if (isSamePassword)
                throw new BadRequestException("New password cannot be the same as the old password");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordRequest.NewPassword);

                if (!result.Succeeded)
                {
                    string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }

                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                await _userManager.UpdateAsync(user);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password transaction failed for user {Email}", resetPasswordRequest.Email);
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        #endregion

        #region GetUserByEmailAsync
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        #endregion

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _userManager.GenerateEmailConfirmationTokenAsync((ApplicationUser)user);
        }


        public async Task<LoginResDto> LoginWithGoogle(GoogleLoginRequest googleLoginRequest, CancellationToken cancellationToken)
        {
            var payload = await _jwtProvider.VerifyGoogleToken(googleLoginRequest.IdToken);
            if (payload is null)
                throw new BadRequestException("Invalid Google token");

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    PersonName = payload.Name,
                    EmailConfirmed = true
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }
                var defaultRole = RoleType.User.ToString();
                await _userManager.AddToRoleAsync(user, defaultRole);
            }

            var roles = await _userManager.GetRolesAsync(user);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                string refreshToken = GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenDays);
                RefreshToken refreshTokenRecord = new RefreshToken
                {
                    Token = refreshToken,
                    ExpirationAt = refreshTokenExpiry
                };
                user.RefreshTokens.Add(refreshTokenRecord);
                await _userManager.UpdateAsync(user);
                var (token, expiresIn) = _jwtProvider.GenerateToken(user, roles, refreshTokenRecord);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return new LoginResDto(
                    Id: user.Id,
                    Email: user.Email!,
                    PersonName: user.PersonName,
                    Token: token,
                    ExpiresIn: expiresIn,
                    Refreshtoken: refreshToken,
                    Roles: roles,
                    RefreshTokenExpirationDate: refreshTokenExpiry,
                    MyGym: null
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google login transaction failed");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        // ============ EMAIL CONFIRMATION OTP (جديد بالكامل) ============
        public async Task<string> GenerateEmailConfirmationOtpAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var appUser = (ApplicationUser)user;
            string otp = RandomNumberGenerator.GetInt32(10000, 99999).ToString();

            appUser.EmailConfirmationOtp = HashOtp(otp);
            appUser.EmailConfirmationOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            appUser.EmailConfirmationOtpAttempts = 0;
            await _userManager.UpdateAsync(appUser);

            return otp;
        }

        #region Helper
        private static string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private static string HashOtp(string value)
        {
            return System.Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(value)
                )
            );
        }

        private static bool VerifyOtpHash(string plainText, string hashedValue)
        {
            var hash = System.Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(plainText)
                )
            );
            return string.Equals(hash, hashedValue, StringComparison.Ordinal);
        }
        #endregion
    }
}
