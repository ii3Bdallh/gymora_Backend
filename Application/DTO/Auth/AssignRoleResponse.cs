namespace Application.DTO.Auth
{
    /// <summary>
    /// Response for role assignment operations
    /// </summary>
    public record AssignRoleResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string? ErrorMessage { get; init; }
    }
}
