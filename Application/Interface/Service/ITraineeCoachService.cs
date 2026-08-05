using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;

using Application.DTO.Pagintion;
using Domain.Model;

namespace Application.Interface.Service;

public interface ICoachAssignmentService : IBaseService<CoachAssignment, CoachAssignmentRDTO, CoachAssignmentCDTO, CoachAssignmentUDTO>
{

}
