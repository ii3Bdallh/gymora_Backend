using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IOwnerSubscriptionRepo : IBaseRepo<OwnerSubscription>
    {
        Task<bool> HasActiveSubscriptionAsync(int ownerUserId, CancellationToken ct = default);
        Task<bool> HasGraceSubscriptionAsync(int ownerUserId, CancellationToken ct = default);
        Task<OwnerSubscription?> GetCurrentSubscriptionAsync(int ownerUserId, CancellationToken ct = default);
    }
}