using System.Reflection;
using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Base;

namespace Infrastructure.Cache
{
    public class FilterPropertyMeta
    {
        public string Name { get; set; } = "";
        public FilterType FilterType { get; set; }
        public Type PropertyType { get; set; } = typeof(object);
        public Type UnderlyingType { get; set; } = typeof(object);
        public bool IsNullable { get; set; }
    }

    public class QueryCache
    {
        private readonly Dictionary<Type, string?> _searchCache = new();
        private readonly Dictionary<Type, HashSet<string>> _sortCache = new();
        private readonly Dictionary<Type, Dictionary<string, FilterPropertyMeta>> _filterCache = new();

        public QueryCache()
        {
            var entityTypes = Assembly.GetAssembly(typeof(IBaseEntity))!
                .GetTypes()
                .Where(t => typeof(IBaseEntity).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in entityTypes)
            {
                BuildSearchCache(type);
                BuildFilterCache(type);
            }
        }

        // =====================
        // Search
        // =====================
        private void BuildSearchCache(Type type)
        {
            var properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(SearchableAttribute)) &&
                            p.PropertyType == typeof(string))
                .ToList();

            _searchCache[type] = properties.Any()
                ? string.Join(" OR ", properties.Select(p => $"{p.Name}.Contains(@0)"))
                : null;
        }

        public string? GetSearchCondition(Type type) =>
            _searchCache.TryGetValue(type, out var condition) ? condition : null;

    

        // =====================
        // Filter
        // =====================
        private void BuildFilterCache(Type type)
        {
            var propsDict = new Dictionary<string, FilterPropertyMeta>();
            
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(FilterableAttribute)));

            foreach (var prop in props)
            {
                var propType = prop.PropertyType;
                var isNullable = propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
                var underlyingType = isNullable ? Nullable.GetUnderlyingType(propType)! : propType;

                var meta = new FilterPropertyMeta
                {
                    Name = prop.Name,
                    PropertyType = propType,
                    UnderlyingType = underlyingType,
                    IsNullable = isNullable,
                    FilterType = prop.GetCustomAttribute<FilterableAttribute>()!.FilterType
                };

                propsDict[prop.Name.ToLower()] = meta;
            }

            _filterCache[type] = propsDict;
        }

        public Dictionary<string, FilterPropertyMeta> GetFilterableProperties(Type type) =>
            _filterCache.TryGetValue(type, out var props) ? props : new Dictionary<string, FilterPropertyMeta>();
    }
}