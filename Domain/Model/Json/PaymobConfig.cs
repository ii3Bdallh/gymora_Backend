namespace Domain.Model.Json
{
    public class PaymobConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string IntegrationId { get; set; } = string.Empty;
        public string HmacSecret { get; set; } = string.Empty;
    }
}
