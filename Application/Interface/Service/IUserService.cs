using Gymora.Contracts.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Service
{
    public interface IUserService
    {
        Task<UserProfileRDTO> GetUserProfileAsync(int userId, CancellationToken cancellationToken);
        Task<UserProfileRDTO> UpdateUserProfileAsync(int userId, UserProfileUDTO updateDto, CancellationToken cancellationToken);
    }
}
