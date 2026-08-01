using System;

namespace Domain.Events
{
    public record PasswordResetCompletedEvent(int UserId, string Email);
}
