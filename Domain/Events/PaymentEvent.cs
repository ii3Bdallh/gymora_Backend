namespace Domain.Events;

public record PaymentCreatedEvent(int PaymentRequestId);

public record PaymentApprovedEvent(int PaymentRequestId , int UserId);

public record PaymentRejectedEvent(int PaymentRequestId, int UserId, string RejectionReason);