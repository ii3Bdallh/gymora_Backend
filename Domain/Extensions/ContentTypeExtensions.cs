using Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Extensions
{
    /// <summary>
    /// Extension methods for FileType enum
    /// Provides utilities to get file extensions and MIME types
    /// </summary>
    public static class FileTypeExtensions
    {
        /// <summary>
        /// Get the file extension for a content type
        /// </summary>
        public static string GetFileExtension(this FileType FileType)
        {
            return FileType switch
            {
                FileType.Video => ".mp4",
                FileType.PDF => ".pdf",
                FileType.Audio => ".mp3",
                FileType.Document => ".docx",
                FileType.Image => ".jpg",
                FileType.Other => ".bin",
                _ => ".bin"
            };
        }

        /// <summary>
        /// Get the MIME type for a content type
        /// </summary>
        public static string GetMimeType(this FileType FileType)
        {
            return FileType switch
            {
                FileType.Video => "video/mp4",
                FileType.PDF => "application/pdf",
                FileType.Audio => "audio/mpeg",
                FileType.Document => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                FileType.Image => "image/jpeg",
                FileType.Other => "application/octet-stream",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// Get all allowed file extensions for a content type
        /// </summary>
        public static string[] GetAllowedExtensions(this FileType FileType)
        {
            return FileType switch
            {
                FileType.Video => new[] { ".mp4", ".avi", ".mov", ".mkv" },
                FileType.PDF => new[] { ".pdf" },
                FileType.Audio => new[] { ".mp3", ".wav", ".m4a", ".flac" },
                FileType.Document => new[] { ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt" },
                FileType.Image => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" },
                FileType.Other => new[] { ".zip", ".rar", ".7z" },
                _ => new[] { ".bin" }
            };
        }

        /// <summary>
        /// Get description of the content type
        /// </summary>
        public static string GetDescription(this FileType FileType)
        {
            return FileType switch
            {
                FileType.Video => "Video files (MP4, AVI, MOV, MKV)",
                FileType.PDF => "PDF Documents",
                FileType.Audio => "Audio files (MP3, WAV, M4A, FLAC)",
                FileType.Document => "Office documents (DOCX, XLSX, PPTX)",
                FileType.Image => "Image files (JPG, PNG, GIF, BMP, WEBP)",
                FileType.Other => "Other files (ZIP, RAR, 7Z)",
                _ => "Unknown type"
            };
        }

        /// <summary>
        /// Determine content type from file extension
        /// </summary>
        public static FileType GetFileTypeFromExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return FileType.Other;

            var extension = System.IO.Path.GetExtension(fileName).ToLower();

            return extension switch
            {
                // Video extensions
                ".mp4" or ".avi" or ".mov" or ".mkv" => FileType.Video,
                
                // PDF extensions
                ".pdf" => FileType.PDF,
                
                // Audio extensions
                ".mp3" or ".wav" or ".m4a" or ".flac" => FileType.Audio,
                
                // Document extensions
                ".docx" or ".doc" or ".xlsx" or ".xls" or ".pptx" or ".ppt" => FileType.Document,
                
                // Image extensions
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => FileType.Image,
                
                // Other extensions
                ".zip" or ".rar" or ".7z" => FileType.Other,
                
                _ => FileType.Other
            };
        }
    }
}
