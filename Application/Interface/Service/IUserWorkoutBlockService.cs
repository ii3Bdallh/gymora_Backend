using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Model;
using Application.Interface.Service;
using Domain.Model;

namespace Application.Interface.Service
{
    public interface IUserWorkoutBlockService : IBaseService<UserWorkoutBlock, UserWorkoutBlockRDTO, UserWorkoutBlockCDTO, UserWorkoutBlockUDTO>
    {
        Task UnblockUserAsync(int userId, CancellationToken cancellationToken);
    }
}
