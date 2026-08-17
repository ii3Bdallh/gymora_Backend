
using System.ComponentModel.DataAnnotations;
using Application.Common.FileValidation;
using Application.DTO.Base;
using Domain.Enum;
using Domain.Model;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Model
{
    public record GymCDTO : BaseFCDTO
    {

        [AllowedFileTypes(10, AllowedFileType.Jpg, AllowedFileType.Png)]
        public override IFormFile? File { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]

        public string? Email { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine1 cannot exceed 200 characters.")]
        public string? AddressLine1 { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine2 cannot exceed 200 characters.")]
        public string? AddressLine2 { get; set; }
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }
        [MaxLength(20, ErrorMessage = "PostalCode cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }


        [Required(ErrorMessage = "Latitude is required.")]
        public decimal Latitude { get; set; }
        [Required(ErrorMessage = "Longitude is required.")]
        public decimal Longitude { get; set; }

        // public GymStatus Status { get; set; } = GymStatus.Active;

        public int OwnerUserId { get; set; }


    }

    public record GymUDTO : BaseFUDTO
    {
        [AllowedFileTypes(10, AllowedFileType.Jpg, AllowedFileType.Png)]
        public override IFormFile? File { get; set; }

        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? Name { get; set; }
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine1 cannot exceed 200 characters.")]
        public string? AddressLine1 { get; set; }
        [MaxLength(200, ErrorMessage = "AddressLine2 cannot exceed 200 characters.")]
        public string? AddressLine2 { get; set; }
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }
        [MaxLength(20, ErrorMessage = "PostalCode cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public GymStatus Status { get; set; }
    }

    public record GymRDTO : BaseFRDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public GymStatus Status { get; set; }

    }

    public sealed class SwitchGymRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "GymId must be greater than 0.")]
        public int GymId { get; set; }

        [Required(ErrorMessage = "RefreshToken is required.")]
        public string RefreshToken { get; set; } = null!;

        [Required(ErrorMessage = "AccessToken is required.")]
        public string AccessToken { get; set; } = null!;
    }



    public sealed record ChangeOwnerDTO
    {
        [Required]
        public int GymId { get; init; }

        [Required]
        public int NewOwnerUserId { get; init; }
    }

    public class MyGymDto
    {
        public int GymId { get; set; }

        public int GymPeopleId { get; set; }

        public string GymName { get; set; } = default!;

        public string GymRole { get; set; } = default!;

        public bool HasAccess { get; set; }

        public string? DeniedReason { get; set; }

    }



    public class UserGymRDTO
    {
        public int GymId { get; set; }
        public string GymName { get; set; } = null!;
        public string? LogoUrl { get; set; }

        public int GymPersonId { get; set; }

        public int OwnerUserId { get; set; }
        public GymRole GymRole { get; set; }
        public GymAccessStatus GymAccessStatus { get; set; }
        public GymStatus GymStatus { get; set; }
        public bool IsAccessible { get; set; }
        public string? InaccessibleReason { get; set; }

        public GymPersonAccessStatus? PersonAccessStatus { get; set; }

        public DateTime? MembershipEndDate { get; set; }
        public bool HasActiveMembership => MembershipEndDate > DateTime.UtcNow;
    }

    public enum GymAccessStatus
    {
        Active,
        GymSuspended,
        OwnerPlanLimitReached,
        OwnerSubscriptionSuspended,
        PersonSuspended,
        PersonBlocked,
        LeftGym,
        MembershipGrace,
        MembershipExpired,
        MembershipFrozen,
        MembershipCancelled,
        OwnerNotFound,
        StaffSalaryNotPaid
    }

    public class UserGymsListRDTO
    {
        public List<UserGymRDTO> Gyms { get; set; } = new();
        public bool HasActivePlatformSubscription { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

}
