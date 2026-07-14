using Application.DTO.Model;
using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Application.Interface.Service
{
    public interface IPaymentRequestService : IBaseService<PaymentRequest, PaymentRequestRDTO, PaymentRequestCDTO, PaymentRequestUDTO>
    {
        public Task<PaymentRequestRDTO> ApproveAsync(int id, PaymentRequestApprove dto, CancellationToken ct = default);
        public Task<PaymentRequestRDTO> RejectAsync(int id, PaymentRequestReject dto, CancellationToken ct = default);
    }
}