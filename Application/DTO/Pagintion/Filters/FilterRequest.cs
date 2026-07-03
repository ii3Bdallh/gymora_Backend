namespace Application.DTO.Filters
{
    /// <summary>
    /// فلتر نطاق — Min و Max اختياريين
    /// مثال: { "min": "100", "max": "500" }
    /// مثال: { "min": "2024-01-01" }  بدون حد أقصى
    /// </summary>
    public class BetweenFilter
    {
        public string? Min { get; set; }
        public string? Max { get; set; }
    }

    /// <summary>
    /// الـ Request الكامل
    /// </summary>
    public class FilterRequest
    {
        /// <summary>
        /// نطاق — مثال: { "price": { "min": "100", "max": "500" } }
        /// </summary>
        public Dictionary<string, BetweenFilter> BetweenFilters { get; set; } = new();

        /// <summary>
        /// IN list — مثال: { "categoryId": ["1", "2", "3"] }
        /// </summary>
        public Dictionary<string, List<string>> ExactFilters { get; set; } = new();
    }
}