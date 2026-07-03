namespace Application.DTO.Response
{
    public record ConfirmEmailResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public record ForgotPasswordResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public record VerifyOtpResponseDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }

    public record ResetPasswordResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
