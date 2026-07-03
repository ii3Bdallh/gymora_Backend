using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Auth
{
    public record LogoutRequest
    {
        public int UserId { get; init; }

        public string? RefreshToken { get; set; }

        public bool LogoutFromAllDevices { get; init; } = false;
    }
}
