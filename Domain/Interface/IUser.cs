namespace Domain.Interface;

public interface IUser
{
    int Id { get; }
    string? Email { get; }
    string PersonName { get; }
}
