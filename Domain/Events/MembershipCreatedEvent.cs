using System;

namespace Domain.Events;

/// <summary>
/// Published when a gym member's membership is created (on invitation accept or manual assignment).
/// Will be consumed in the future to auto-create a Revenue record in the Finance module.
/// </summary>
public record MembershipCreatedEvent
{
    public int GymPersonId { get; init; }
    public int GymId { get; init; }
    public int MemberUserId { get; init; }
    public string PlanName { get; init; } = null!;
    public int DurationDays { get; init; }
    public decimal PricePaid { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal FinalAmount { get; init; }
    public DateTime MembershipStartDate { get; init; }
    public DateTime MembershipEndDate { get; init; }
    public int CreatedByUserId { get; init; }
    public int CreatedByPersonId { get; init; }
}
