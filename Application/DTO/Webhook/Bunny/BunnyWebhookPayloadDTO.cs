using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Application.DTO.Bunny
{
    /// <summary>
    /// Represents the webhook payload sent by Bunny.net when an upload completes or fails
    /// </summary>
    public record BunnyWebhookPayloadDTO
    {
        public int VideoLibraryId { get; init; }
        public string VideoGuid { get; init; } = string.Empty;
        public BunnyUploadStatus Status { get; init; }
    }
}


