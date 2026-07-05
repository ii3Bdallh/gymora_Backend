using MediatR;

namespace Domain.Events;

public sealed record TestEvent(
    int UserId,
    string Email,
    string Message) : INotification;
