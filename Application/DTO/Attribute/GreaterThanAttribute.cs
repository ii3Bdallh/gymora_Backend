using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTO.Attribute
{
    /// <summary>
    /// Validates that a DateTime value is greater than another DateTime property value
    /// </summary>
    public class GreaterThanAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        /// <summary>
        /// Initialize the attribute with the property name to compare against
        /// </summary>
        /// <param name="comparisonProperty">The name of the property to compare with</param>
        public GreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be greater than {_comparisonProperty}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Validation only applies if the current value is not null
            if (value is not DateTime dateValue)
                return ValidationResult.Success;

            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (comparisonProperty == null)
                return new ValidationResult($"Unknown property: {_comparisonProperty}");

            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);
            
            // If comparison value is null, validation passes (optional comparison)
            if (comparisonValue is not DateTime comparisonDate)
                return ValidationResult.Success;

            if (dateValue > comparisonDate)
                return ValidationResult.Success;

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
        }
    }
}
