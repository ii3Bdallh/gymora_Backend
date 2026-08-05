namespace Domain.Events;

public record CoachAssignedToMemberEvent(int AssignmentId, int MemberId, int CoachStaffId);
