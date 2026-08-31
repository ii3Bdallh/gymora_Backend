using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Application.DTO.Base;
using Application.DTO.Pagintion;

namespace Application.DTO.Model
{
    // --- Request DTOs (Paged Search Requests) ---



    public class MemberAttendanceHistoryPagedReq : PaginatedSearchReq
    {
        [Required(ErrorMessage = "MemberId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
        public int MemberId { get; set; }
    }

    // --- Response DTOs (RDTOs) ---

    public record AttendanceLogItemRDTO : BaseGymRDTO
    {
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = null!;
        public string DisplayId { get; set; } = null!;
        public string MembershipStatus { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
        public int? RecordedById { get; set; }
        public string? RecordedByStaffName { get; set; }
    }

    public record GymAttendanceHistoryListRDTO
    {
        public List<AttendanceLogItemRDTO> Records { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    // --- Dashboard DTOs ---

    public record AttendanceDashboardStatsRDTO(
        int TodayCheckInsCount,
        int CurrentlyInsideCount,
        int ActiveMembersCount,
        int ExpiredMembershipsCount
    );

    public record RecentCheckInItemRDTO
    {
        public int AttendanceId { get; set; }
        public int MemberId { get; set; }
        public string MemberFullName { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public string PlanType { get; set; } = null!;
        public DateTime CheckInTime { get; set; }
    }

    public record GymAttendanceDashboardRDTO
    {
        public int GymId { get; set; }
        public AttendanceDashboardStatsRDTO Stats { get; set; } = null!;
        public List<RecentCheckInItemRDTO> RecentEntries { get; set; } = new();
    }

    public record AttendanceReportRDTO
    {
        public int TotalCheckIns { get; set; }
        public decimal PercentageChangeVsLastPeriod { get; set; }
        public List<FinancialChartPointRDTO> ChartDataPoints { get; set; } = new();
        public List<AttendanceLogItemRDTO> RecentTransactions { get; set; } = new();
        public AttendanceDashboardStatsRDTO? Stats { get; set; }
    }

    // --- Request DTO (Record Check-In DTO) ---

    public record RecordCheckInCDTO : BaseGymCDTO
    {
        [Required(ErrorMessage = "MemberId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid MemberId")]
        public int MemberId { get; init; }

        public string EntryMethod { get; init; } = "Staff Override";
    }

    public record RecordCheckInUDTO : BaseGymUDTO
    {
    }

    // --- Search Members DTOs ---

    public record CheckInMemberItemRDTO
    {
        public int MemberId { get; set; }
        public string FullName { get; set; } = null!;
        public string DisplayId { get; set; } = null!;
        public string MembershipStatus { get; set; } = null!;
        public string PlanName { get; set; } = null!;
        public DateTime? MembershipEndDate { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public record CheckInMemberListRDTO(
        List<CheckInMemberItemRDTO> Members,
        int TotalCount
    );
}
