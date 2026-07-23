using System;

namespace Domain.Events;

public record SalaryPaidEvent(int StaffId, decimal Amount, DateTime PaidAt, int GymId);
