namespace Domain.Events;

public record SubscriptionActivatedEvent(int SubscriptionId, int PaymentRequestId, int OwnerUserId);