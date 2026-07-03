using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Bunny;

namespace Application.Interface.Service.Shared
{
    /// <summary>
    /// Interface for Bunny Stream Collection management
    /// Provides CRUD operations for collections
    /// </summary>
    public interface IBunnyCollectionService
    {
        /// <summary>
        /// Get a collection by ID
        /// </summary>
        /// <param name="collectionId">The collection GUID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection details</returns>
        Task<BunnyCollectionDTO> GetCollectionAsync(string collectionId, CancellationToken cancellationToken = default);



        /// <summary>
        /// Create a new collection
        /// </summary>
        /// <param name="dto">Collection creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created collection details</returns>
        Task<BunnyCollectionDTO> CreateCollectionAsync(CreateBunnyCollectionDTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing collection
        /// </summary>
        /// <param name="collectionId">The collection GUID</param>
        /// <param name="dto">Collection update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated collection details</returns>
        Task UpdateCollectionAsync(string collectionId, UpdateBunnyCollectionDTO dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a collection
        /// </summary>
        /// <param name="collectionId">The collection GUID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteCollectionAsync(string collectionId, CancellationToken cancellationToken = default);
    }
}

