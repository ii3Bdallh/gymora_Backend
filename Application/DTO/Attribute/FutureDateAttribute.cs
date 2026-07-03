using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Attribute
{
    /// <summary>
    /// Validates that a DateTime value is in the future (greater than current time)
    /// </summary>
    public class FutureDateAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a future date and time.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime > DateTime.UtcNow)
                    return ValidationResult.Success;

                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}
