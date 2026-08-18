namespace Domain.Model;

public class UserDevice
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string DeviceToken { get; set; } = default!;

    public string DeviceType { get; set; } = default!;

    public string AppVersion { get; set; } = default!;

    public DateTime LastUsedAt { get; set; }


    public DateTime CreatedAt { get; set; }
}
