using Domain.Model.Base;

namespace Domain.Model;

public class TestEntity : EventEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = default!;
}
