using System.ComponentModel.DataAnnotations;
namespace Application.DTO
{
    public record NotificationDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters")]
        public required string Title { get; init; }

        [Required(ErrorMessage = "Body is required")]
        [StringLength(100, ErrorMessage = "Body cannot exceed 100 characters")]
        public required string Body { get; init; }

        public NotificationData? Data { get; init; }

        public Dictionary<string, string>? ConvertDataToDictionary()
        {
            if (Data == null)
                return null;

            var dict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(Data.ClickAction))
                dict["click_action"] = Data.ClickAction;

            if (!string.IsNullOrEmpty(Data.Status))
                dict["status"] = Data.Status;

            if (!string.IsNullOrEmpty(Data.Timestamp))
                dict["timestamp"] = Data.Timestamp;

            return dict;
        }
    }

    public record NotificationData
    {
        public string? ClickAction { get; init; }
        public string? Status { get; init; }
        public string? Timestamp { get; init; }
    }
}

