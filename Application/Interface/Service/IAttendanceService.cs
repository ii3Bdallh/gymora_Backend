using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.DTO.Pagintion;
using Domain.Model;

namespace Application.Interface.Service;

public interface IAttendanceService : IBaseService<Attendance, AttendanceLogItemRDTO, RecordCheckInCDTO, RecordCheckInUDTO>
{
    Task<GymAttendanceDashboardRDTO> GetDashboardAsync(int gymId, CancellationToken ct);
    // Task RecordCheckInAsync(int gymId, RecordCheckInCDTO dto, CancellationToken ct);
    // Task<CheckInMemberListRDTO> SearchMembersForCheckInAsync(int gymId, string searchTerm, CancellationToken ct);
    // Task<PaginatedRes<AttendanceLogItemRDTO>> GetMemberAttendanceAsync(MemberAttendanceHistoryPagedReq req, CancellationToken ct);
}
