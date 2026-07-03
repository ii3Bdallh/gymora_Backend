using System.ComponentModel.DataAnnotations;

namespace Api.Extensions
{
    public static class ValidationHelper
    {
        public static List<ValidationResult> ValidateList<T>(IEnumerable<T> list)
        {
            var allResults = new List<ValidationResult>();
            foreach (var item in list)
            {
                var context = new ValidationContext(item);
                var results = new List<ValidationResult>();
                Validator.TryValidateObject(item, context, results, true);
                allResults.AddRange(results);
            }
            return allResults;
        }
    }

}
