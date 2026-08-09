using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Pagintion;

namespace Application.DTO.Model;

public class GetAssignedMemberForCoachPagedReq : PaginatedSearchReq
{
    [Required(ErrorMessage = "Coach Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid CoachId")]
    public int CoachId { get; set; }

    [Required(ErrorMessage = "Gym Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
    public int GymId { get; set; }
}

public class GetAssignCoachForMemberPagedReq : PaginatedSearchReq
{
    [Required(ErrorMessage = "Member Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Gym Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
    public int GymId { get; set; }
}


// public class GetAllAssignmentForGym : PaginatedSearchReq
// {
//     [Required(ErrorMessage = "Gym Id is required")]
//     [Range(1, int.MaxValue, ErrorMessage = "Invalid GymId")]
//     public int GymId { get; set; }
// }



public record CoachAssignmentRDTO : BaseGymRDTO
{
    public int MemberId { get; set; }
    public GymPersonRDTO? Member { get; set; }

    public int CoachStaffId { get; set; }
    public GymPersonRDTO? CoachStaff { get; set; }

    public int AssignedById { get; set; }
    public GymPersonRDTO? AssignedBy { get; set; }

    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}



public record CoachAssignmentCDTO : BaseGymCDTO
{
    [Required(ErrorMessage = "Member Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "GymPerson Id is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid CoachStaffId")]
    public int CoachStaffId { get; set; }
};


public record CoachAssignmentUDTO : BaseGymUDTO
{
    
}


// public class CoachAssignment : BaseGymEntity
// {
//     public int MemberId { get; set; }
//     public GymPerson Member { get; set; } = null!;

//     public int CoachStaffId { get; set; }
//     public GymPerson Coach { get; set; } = null!;

//     public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
//     public int AssignedById { get; set; }
//     public GymPerson AssignedBy { get; set; } = null!;

//     public DateTime? EndedAt { get; set; }
// }