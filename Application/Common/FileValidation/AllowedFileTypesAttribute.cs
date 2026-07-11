using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.Common.FileValidation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowedFileTypesAttribute : ValidationAttribute
    {
        private readonly AllowedFileType[] _allowedTypes;
        private readonly long _maxSizeBytes;

        public AllowedFileTypesAttribute(long maxSizeMb, params AllowedFileType[] allowedTypes)
        {
            _allowedTypes = allowedTypes;
            _maxSizeBytes = maxSizeMb * 1024 * 1024;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is null)
                return ValidationResult.Success;

            if (value is not IFormFile file)
                return new ValidationResult("Invalid file.");

            if (file.Length == 0)
                return new ValidationResult("File is empty.");

            if (file.Length > _maxSizeBytes)
                return new ValidationResult($"File exceeds the maximum size of {_maxSizeBytes / (1024 * 1024)}MB.");

            string extension = Path.GetExtension(file.FileName);

            var matchingType = _allowedTypes
                .Select(t => (Type: t, Info: FileSignatures.Map[t]))
                .FirstOrDefault(x => x.Info.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase));

            if (matchingType.Info.Extension is null)
            {
                string allowedList = string.Join(", ", _allowedTypes.Select(t => FileSignatures.Map[t].Extension));
                return new ValidationResult($"Only these file types are allowed: {allowedList}.");
            }

            byte[] signature = matchingType.Info.Signature;
            byte[] header = new byte[signature.Length];

            using (Stream stream = file.OpenReadStream())
            {
                int read = stream.Read(header, 0, header.Length);
                if (read < header.Length || !header.SequenceEqual(signature))
                    return new ValidationResult("File content does not match its extension (possibly renamed or corrupted).");
            }

            return ValidationResult.Success;
        }
    }
}
