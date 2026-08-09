using Domain.Model;

namespace Application.Interface.Repo;

public interface IAttendanceRepo : IBaseRepo<Attendance>
{
    Task<int> GetTodayCheckInsCountAsync(int gymId, CancellationToken ct = default);
    Task<int> GetCurrentlyInsideCountAsync(int gymId, CancellationToken ct = default);
    Task<List<Attendance>> GetRecentEntriesAsync(int gymId, int count, CancellationToken ct = default);
    // Task<List<Attendance>> GetAttendanceReportRecordsAsync(int gymId, DateTime fromDate, DateTime toDate, CancellationToken ct = default);
}
