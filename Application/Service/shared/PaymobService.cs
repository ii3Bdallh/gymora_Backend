using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Webhook;
using Application.Interface.Service.Shared;
using Domain.Model.Json;
using Microsoft.Extensions.Logging;

namespace Application.Service.Shared
{
    public class PaymobService(PaymobConfig paymobConfig, HttpClient httpClient, ILogger<PaymobService> logger) : IPaymobService
    {
        private readonly PaymobConfig _config = paymobConfig;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<PaymobService> _logger = logger;

        public async Task<string> GetPaymentKeyAsync(int orderId, CancellationToken ct = default)
        {
            // Authenticate
            var authReq = new { api_key = _config.ApiKey };
            var authResp = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens", authReq, ct);
            authResp.EnsureSuccessStatusCode();
            using var authDoc = await authResp.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct) ?? JsonDocument.Parse("{}");
            var token = authDoc.RootElement.GetProperty("token").GetString() ?? string.Empty;

            // Create payment key request. Note: the real implementation should set accurate amount/currency/billing data.
            var paymentKeyReq = new
            {
                expiration = 3600,
                auth_token = token,
                order_id = orderId.ToString(),
                integration_id = _config.IntegrationId,
                amount_cents = "100", // placeholder; replace with actual amount in cents
                currency = "EGP",
                billing_data = new
                {
                    first_name = "NA",
                    last_name = "NA",
                    email = "na@example.com",
                    phone_number = "NA",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "NA",
                    state = "NA"
                }
            };

            var payResp = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", paymentKeyReq, ct);
            payResp.EnsureSuccessStatusCode();
            using var payDoc = await payResp.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct) ?? JsonDocument.Parse("{}");
            var paymentToken = payDoc.RootElement.TryGetProperty("token", out var t) ? t.GetString() ?? string.Empty : string.Empty;
            return paymentToken;
        }

        public Task HandleWebhookAsync(PaymobWebhookDto webhook, string hmacHeader, string rawBody, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_config.HmacSecret))
            {
                _logger.LogWarning("Paymob HMAC secret is not configured.");
                return Task.CompletedTask;
            }

            // Compute HMAC SHA512 of raw body
            var secretBytes = Encoding.UTF8.GetBytes(_config.HmacSecret);
            using var hmac = new HMACSHA512(secretBytes);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var computedHex = BitConverter.ToString(computed).Replace("-", "").ToLowerInvariant();

            if (!string.Equals(computedHex, (hmacHeader ?? string.Empty).ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid Paymob webhook HMAC. Received: {Header}, Computed: {Computed}", hmacHeader, computedHex);
                return Task.CompletedTask;
            }

            _logger.LogInformation("Valid Paymob webhook received: type={Type} id={Id} success={Success}",
                webhook?.Type, webhook?.Obj?.Id, webhook?.Obj?.Success);

            // TODO: Add domain-specific handling (update order status, record transaction, etc.)
            return Task.CompletedTask;
        }
    }
}