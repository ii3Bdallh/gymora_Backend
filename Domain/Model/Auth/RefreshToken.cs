namespace Domain.Model;

public class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = string.Empty;

    public int CurrentGymId { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpirationAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpirationAt;

    public bool IsValid => RevokedAt is null && !IsExpired;

    public int UserId { get; set; }
}
