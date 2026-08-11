using System;

namespace Domain.Events;

/// <summary>
/// Published when a staff member's salary is paid.
/// Will be consumed in the future to auto-create an Expenses record in the Finance module.
/// </summary>
public record SalaryPaidEvent
{
    public int GymPersonId { get; init; }
    public int GymId { get; init; }
    public int StaffUserId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaidAt { get; init; }
    public DateTime PeriodFrom { get; init; }
    public DateTime PeriodTo { get; init; }
    public int PaidByUserId { get; init; }
}
