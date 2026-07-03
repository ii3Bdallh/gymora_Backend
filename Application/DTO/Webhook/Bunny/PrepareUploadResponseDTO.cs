using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interface.Service.Shared;

namespace Application.DTO.Bunny
{
    /// <summary>
    /// Response DTO for prepare upload endpoints
    /// Contains upload credentials for Bunny Stream (video) or Bunny Storage
    /// </summary>
    public record PrepareUploadResponseDTO
    {
        /// <summary>
        /// The content ID created in the database
        /// </summary>
        public int ContentId { get; init; }

        public BunnyUploadCredentials? bunnyUploadCredentials { get; init; } 
        /// <summary>
        /// Timestamp when content was created
        /// </summary>
        public DateTime CreatedAt { get; init; }
    }
}
