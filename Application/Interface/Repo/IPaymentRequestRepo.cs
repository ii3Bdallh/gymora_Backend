using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IPaymentRequestRepo : IBaseRepo<PaymentRequest>
    {

        Task<bool> HasPendingRequestAsync(int UserId, CancellationToken ct = default);
    }
}