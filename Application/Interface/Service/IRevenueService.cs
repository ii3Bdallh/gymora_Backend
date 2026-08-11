using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IRevenueService : IBaseService<Revenue, RevenueRDTO, RevenueCDTO, RevenueUDTO>
    {
    }
}
