using System;
using System.Collections.Generic;

namespace Application.DTO.Bunny
{
    /// <summary>
    /// DTO representing a Bunny Stream Collection
    /// </summary>
    public class BunnyCollectionDTO
    {
        public long VideoLibraryId { get; set; }
        
        public string Guid { get; set; } = string.Empty;
        
        public string Name { get; set; } = string.Empty;
        
        public int VideoCount { get; set; }
        
        public long TotalSize { get; set; }
        
        public List<string>? PreviewVideoIds { get; set; }
        
        public List<string> PreviewImageUrls { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a new Bunny Stream Collection
    /// </summary>
    public class CreateBunnyCollectionDTO
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating a Bunny Stream Collection
    /// </summary>
    public class UpdateBunnyCollectionDTO
    {
        public string Name { get; set; } = string.Empty;
    }
}
