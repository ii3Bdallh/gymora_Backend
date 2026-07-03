using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Bunny
{
    /// <summary>
    /// Response DTO for file access/download endpoint
    /// Contains the secure, time-limited URL for accessing a file
    /// </summary>
    public record FileAccessResponseDTO
    {
        /// <summary>
        /// Content ID
        /// </summary>
        public int ContentId { get; init; }

        /// <summary>
        /// Content title
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Content type (PDF, Image, Audio, Document, etc.)
        /// </summary>
        public string FileType { get; init; } = string.Empty;

        /// <summary>
        /// Secure access URL with token and expiration
        /// </summary>
        public string AccessUrl { get; init; } = string.Empty;


    }
}
