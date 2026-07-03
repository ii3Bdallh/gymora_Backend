using Application.DTO;
using Domain.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IAdminRepo
    {
        /// <summary>
        /// Get all admins with device tokens
        /// </summary>
        Task<List<AppUser>> GetAllAdminsWithDeviceTokensAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get admin by ID with device tokens
        /// </summary>
        Task<AppUser?> GetAdminByIdWithDeviceTokenAsync(int adminId, CancellationToken cancellationToken = default);




    }
}
