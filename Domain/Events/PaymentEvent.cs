namespace Domain.Events;

public record PaymentCreatedEvent(int PaymentRequestId);

public record PaymentApprovedEvent(int PaymentRequestId , int UserId , int? CouponId , decimal? DiscountAmount);

public record PaymentRejectedEvent(int PaymentRequestId, int UserId, string RejectionReason);