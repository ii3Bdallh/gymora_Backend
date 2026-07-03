using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Bunny
{
    /// <summary>
    /// Response DTO for watch video endpoint
    /// Contains the secure, time-limited URL for watching a video
    /// </summary>
    public record WatchVideoResponseDTO
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
        /// Secure watch URL with token and expiration
        /// </summary>
        public string WatchUrl { get; init; } = string.Empty;


    }
}
