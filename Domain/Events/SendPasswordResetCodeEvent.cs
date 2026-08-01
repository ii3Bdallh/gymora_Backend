using System;

namespace Domain.Events
{
    public record SendPasswordResetCodeEvent(string Email, string Code, int ExpirationInMinutes);
}
