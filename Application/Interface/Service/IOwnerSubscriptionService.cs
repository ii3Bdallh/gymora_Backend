using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IOwnerSubscriptionService : IBaseService<OwnerSubscription, OwnerSubscriptionRDTO, OwnerSubscriptionCDTO, OwnerSubscriptionUDTO>
    {
        Task<OwnerSubscriptionRDTO> CreateFromApprovedPaymentAsync(int paymentRequestId, CancellationToken ct = default);
    }
}