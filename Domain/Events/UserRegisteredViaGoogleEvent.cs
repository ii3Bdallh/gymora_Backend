using System;

namespace Domain.Events
{
    public record UserRegisteredViaGoogleEvent(int UserId, string Email, string FullName);
}
