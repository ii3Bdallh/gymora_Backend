namespace Domain.Model;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? GymId { get; set; }

    public string Type { get; set; } = default!;

    public string Title { get; set; } = default!;

    public string Body { get; set; } = default!;

    public string? DataJson { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
