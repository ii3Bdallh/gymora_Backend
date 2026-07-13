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
    }
}