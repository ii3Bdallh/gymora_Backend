namespace Application.DTO.Response
{
    /// <summary>
    /// Response for user adding a role to themselves
    /// </summary>
    public record AddRoleToMeResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public List<string>? CurrentRoles { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
