using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;


namespace Application.DTO.Attribute
{
    public class PngFileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();
                if (ext == ".png" && type == "image/png")
                    return ValidationResult.Success;

                return new ValidationResult("Only PNG image files are allowed.");
            }

            // If it's not a file or is null, return success (use [Required] for mandatory files)
            return ValidationResult.Success;
        }
    }



    public class SvgFileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (ext == ".svg" && type == "image/svg+xml")
                    return ValidationResult.Success;

                return new ValidationResult("Only SVG files are allowed.");
            }

            // If it's not a file or is null, return success (use [Required] for mandatory files)
            return ValidationResult.Success;
        }
    }

    public class JpegFileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if ((ext == ".jpg" || ext == ".jpeg") && type == "image/jpeg")
                    return ValidationResult.Success;

                return new ValidationResult("Only JPEG,JPG image files are allowed.");
            }

            return ValidationResult.Success;
        }
    }



    public class PdfFileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (ext == ".pdf" && type == "application/pdf")
                    return ValidationResult.Success;

                return new ValidationResult("Only PDF files are allowed.");
            }

            return ValidationResult.Success;
        }
    }

    public class DocxFileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (ext == ".docx" && type == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                    return ValidationResult.Success;

                return new ValidationResult("Only DOCX files are allowed.");
            }

            return ValidationResult.Success;
        }
    }

    public class Mp4FileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (ext == ".mp4" && type == "video/mp4")
                    return ValidationResult.Success;

                return new ValidationResult("Only MP4 video files are allowed.");
            }

            return ValidationResult.Success;
        }
    }

    public class Mp3FileOnlyAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (ext == ".mp3" && type == "audio/mpeg")
                    return ValidationResult.Success;

                return new ValidationResult("Only MP3 audio files are allowed.");
            }

            return ValidationResult.Success;
        }
    }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class AllowedFileTypesAttribute : ValidationAttribute
        {
            private readonly string[] _allowedExtensions;
            private readonly string[] _allowedFileTypes;

            public AllowedFileTypesAttribute(string[] allowedExtensions, string[] allowedFileTypes)
            {
                _allowedExtensions = allowedExtensions.Select(e => e.ToLowerInvariant()).ToArray();
                _allowedFileTypes = allowedFileTypes.Select(c => c.ToLowerInvariant()).ToArray();
            }

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value is not IFormFile file)
                    return ValidationResult.Success; // skip if null — use [Required] separately

                var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
                var type = file.ContentType?.ToLowerInvariant();

                if (_allowedExtensions.Contains(ext) && _allowedFileTypes.Contains(type))
                    return ValidationResult.Success;

                return new ValidationResult($"Only files of types: {string.Join(", ", _allowedExtensions)} are allowed.");
            }
        }

        // Backward-compatible alias for older usage.
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class AllowedContentTypesAttribute : AllowedFileTypesAttribute
        {
            public AllowedContentTypesAttribute(string[] allowedExtensions, string[] allowedContentTypes)
                : base(allowedExtensions, allowedContentTypes)
            {
            }
        }


}

