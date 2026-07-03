using Domain.Interface;

namespace Application.Interface.Repo;

public interface IAdminRepo
{
    Task<List<IUser>> GetAllAdminsWithDeviceTokensAsync(CancellationToken cancellationToken = default);

    Task<IUser?> GetAdminByIdWithDeviceTokenAsync(int adminId, CancellationToken cancellationToken = default);
}
