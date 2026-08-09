using Domain.Attributes;
using Domain.Enum;
using Domain.Interface;
using Domain.Model.Auth;
using Domain.Model.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Model
{
    public class GymPerson : BaseGymEntity, IAuditableEntity
    {
        public int? UserId { get; set; } // FK -> ApplicationUser, nullable: unregistered person

        public ApplicationUser? User { get; set; } // Navigation property to ApplicationUser

        [Filterable(FilterType.Exact)]
        public PersonType PersonType { get; set; }

        [Searchable]
        public string Name { get; set; } = null!;

        [Searchable]
        public string PhoneNumber { get; set; } = null!;

        [Searchable]
        public string? Email { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? PhotoUrl { get; set; }

        public Guid InviteCode { get; set; } = Guid.NewGuid();

        // [Filterable(FilterType.Between)]
        // public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Auditing fields
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public int CreatedById { get; set; }


        public GymPersonAccessStatus AccessStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation Properties for profiles
        public GymStaffProfile? StaffProfile { get; set; }
        public GymMemberProfile? MemberProfile { get; set; }


        
    }

    public enum GymPersonAccessStatus
    {
        Active = 1,

        Suspended = 2,

    }
}
