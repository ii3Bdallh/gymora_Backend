namespace Domain.Attributes
{
    public enum FilterType
    {
        /// <summary>
        /// نطاق — Min و Max (أي منهم اختياري)
        /// للأرقام والتواريخ
        /// مثال: Price من 100 لـ 500 / CreatedAt من 2024 فقط
        /// </summary>
        Between,

        /// <summary>
        /// قائمة قيم — IN list
        /// للـ IDs والـ Enums وأي قيم محددة
        /// مثال: CategoryId في [1, 2, 3]
        /// </summary>
        Exact
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class FilterableAttribute : Attribute
    {
        public FilterType FilterType { get; }

        public FilterableAttribute(FilterType filterType)
        {
            FilterType = filterType;
        }
    }
}