using Application.DTO.Model;
using Domain.Model;

namespace Application.Interface.Service;

public interface IMembershipPlanService : IBaseService<MembershipPlan, MembershipPlanRDTO, MembershipPlanCDTO, MembershipPlanUDTO>
{
}
