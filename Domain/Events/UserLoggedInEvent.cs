using System;

namespace Domain.Events
{
    public record UserLoggedInEvent(int UserId, DateTime Timestamp);
}
