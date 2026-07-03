namespace Application.DTO.Bunny
{

    public class BunnyCreateResponse
    {
        public long VideoLibraryId { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime DateUploaded { get; set; }
        public long Views { get; set; }
        public bool IsPublic { get; set; }
        public long Length { get; set; }
        public int Status { get; set; }
        public double Framerate { get; set; }
        public int? Rotation { get; set; }
        public long Width { get; set; }
        public long Height { get; set; }
        public string? AvailableResolutions { get; set; }
        public string? OutputCodecs { get; set; }
        public long ThumbnailCount { get; set; }
        public int EncodeProgress { get; set; }
        public long StorageSize { get; set; }
        public List<object>? Captions { get; set; }
        public bool HasMP4Fallback { get; set; }
        public string CollectionId { get; set; } = string.Empty;
        public string ThumbnailFileName { get; set; } = string.Empty;
        public string? ThumbnailBlurhash { get; set; }
        public long AverageWatchTime { get; set; }
        public long TotalWatchTime { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<object>? Chapters { get; set; }
        public List<object>? Moments { get; set; }
        public List<object>? MetaTags { get; set; }
        public List<object>? TranscodingMessages { get; set; }
        public bool? JitEncodingEnabled { get; set; }
        public int? SmartGenerateStatus { get; set; }
        public bool HasOriginal { get; set; }
        public string? OriginalHash { get; set; }
        public bool? HasHighQualityPreview { get; set; }

        public long StorageSizeMB => StorageSize / (1024 * 1024);

    }
}
