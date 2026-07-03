using System.Text.Json.Serialization;

namespace Application.DTO.Webhook
{
    public class PaymobWebhookDto
    {
        [JsonPropertyName("obj")]
        public PaymobWebhookObj? Obj { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class PaymobWebhookObj
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("order")]
        public PaymobWebhookOrder? Order { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("txn_response_code")]
        public string? TxnResponseCode { get; set; }

        [JsonPropertyName("amount_cents")]
        public string? AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("hmac")]
        public string? Hmac { get; set; }
    }

    public class PaymobWebhookOrder
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}
