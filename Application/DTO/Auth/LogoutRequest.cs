using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Auth
{
    public class LogoutRequest
    {
        public int UserId { get; set; }

        public string? RefreshToken { get; set; }

        public bool LogoutFromAllDevices { get; set; } = false;
    }
}
