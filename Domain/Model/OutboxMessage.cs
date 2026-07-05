namespace Domain.Model;

public class OutboxMessage
{
    public int Id { get; set; }

    public string EventType { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime OccurredAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public int RetryCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
