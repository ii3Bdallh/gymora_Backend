using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Pagintion;

namespace Gymora.Contracts.Authentication
{
    // --- Register DTOs ---

    public class RegisterRequestDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, digit, and special character.")]
        public string Password { get; set; } = null!;
    }

    public class UserInfoDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class AvailableGymDto
    {
        public string GymId { get; set; } = null!;
        public string GymName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class RegisterResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool IsNewUser { get; set; }
        public UserInfoDto User { get; set; } = null!;
        public List<AvailableGymDto> AvailableGyms { get; set; } = new();
    }

    // --- Login DTOs ---

    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class GoogleLoginRequestDto
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }

    public class CurrentGymDto
    {
        public string GymId { get; set; } = null!;
        public string GymName { get; set; } = null!;
        public string Role { get; set; } = null!; // Owner, Coach, Receptionist, Member
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserInfoDto User { get; set; } = null!;
        public CurrentGymDto? CurrentGym { get; set; }
    }

    public class RequireGymSelectionResponseDto
    {
        public string TemporaryAccessToken { get; set; } = null!;
        public UserInfoDto User { get; set; } = null!;
        public List<AvailableGymDto> AvailableGyms { get; set; } = new();
    }

    public class GoogleAuthResponseDto
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool IsNewUser { get; set; }
        public UserInfoDto User { get; set; } = null!;
    }

    // --- Forgot Password DTOs ---

    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class ForgotPasswordResponseDto
    {
        public string Message { get; set; } = null!;
        public int ExpirationInMinutes { get; set; }
    }

    // --- OTP Verification DTOs ---

    public class VerifyOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be exactly 6 digits.")]
        public string Code { get; set; } = null!;
    }

    public class ResendOtpRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class VerifyOtpResponseDto
    {
        public string ResetToken { get; set; } = null!;
        public string Message { get; set; } = null!;
    }

    public class ResendOtpResponseDto
    {
        public string Message { get; set; } = null!;
        public int ExpirationInMinutes { get; set; }
    }

    // --- Reset Password DTOs ---

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string ResetToken { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters with uppercase, lowercase, digit, and special character.")]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ResetPasswordResponseDto
    {
        public string Message { get; set; } = null!;
    }

    // --- Select Gym Workspace DTOs ---

    public class UserGymsPagedReq : PaginatedSearchReq
    {
        public string? StatusFilter { get; set; } // Active, Expired, Locked, Blocked

      
    }



    public class UserGymRDTO
    {
        public string GymId { get; set; } = null!;
        public string GymName { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public string Role { get; set; } = null!; // Owner, Coach, Receptionist, Member
        public string GymStatus { get; set; } = null!; // Active, Expired, Locked, Blocked
        public bool IsAccessible { get; set; }
        public string? InaccessibleReason { get; set; }
    }

    public class UserGymsListRDTO
    {
        public List<UserGymRDTO> Gyms { get; set; } = new();
        public bool HasActivePlatformSubscription { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }


    // --- User Profile DTOs ---

    public record UserProfileRDTO(
        string UserId,
        string FirstName,
        string LastName,
        string FullName,
        string Email,
        string? PhoneNumber,
        string? ProfilePictureUrl,
        DateTime CreatedAt,
        string PlatformRole
    );

    public record UserProfileUDTO(
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        string FirstName,

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        string LastName,

        [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format.")]
        string? PhoneNumber,

        [Url(ErrorMessage = "Profile picture must be a valid URL.")]
        string? ProfilePictureUrl
    );

}
