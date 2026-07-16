using Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.Repo
{
    public interface IGymRepo : IBaseRepo<Gym>
    {
        Task<int> CountOwnedByOwnerAsync(
    int ownerUserId,
    CancellationToken ct = default);


    }
}