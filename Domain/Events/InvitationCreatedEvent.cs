using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Events;

public record InvitationCreatedEvent
{
    public int Id { get; init; }
    public int GymId { get; init; }
    public int UserId { get; init; }
    public string GymRole { get; init; } = null!;
    public int InvitedByUserId { get; init; }
}
