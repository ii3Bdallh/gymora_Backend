using Application.DTO;
using Application.DTO.Webhook;

namespace Application.Interface.Service.Shared
{
    public interface IPaymobService
    {
        Task<string> GetPaymentKeyAsync(int orderId, CancellationToken ct = default);
        Task HandleWebhookAsync(PaymobWebhookDto webhook, string hmacHeader, string rawBody, CancellationToken ct);
    }
}
