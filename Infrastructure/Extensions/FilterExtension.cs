using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using Application.DTO.Exceptions;
using Application.DTO.Filters;
using Domain.Attributes;
using Infrastructure.Cache;
using Infrastructure.Configuration;

namespace Infrastructure.Extensions
{
    public static class FilterExtension
    {
        // Cache مرة واحدة — مش بيتعمل في كل request
        private static readonly MethodInfo _containsMethod =
            typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);

        public static IQueryable<T> ApplyFilters<T>(
            this IQueryable<T> query,
            FilterRequest? filterRequest,
            QueryCache cache)
        {
            if (filterRequest is null)
                return query;

            var allowedProps = cache.GetFilterableProperties(typeof(T));

            query = ApplyBetweenFilters(query, filterRequest.BetweenFilters, allowedProps);
            query = ApplyExactFilters(query, filterRequest.ExactFilters, allowedProps);

            return query;
        }

        // ================================================================
        // Between
        // ================================================================
        private static IQueryable<T> ApplyBetweenFilters<T>(
            IQueryable<T> query,
            Dictionary<string, BetweenFilter> filters,
            Dictionary<string, FilterPropertyMeta> allowedProps)
        {
            foreach (var (key, filter) in filters)
            {
                if (!allowedProps.TryGetValue(key.ToLower(), out var meta) ||
                    meta.FilterType != FilterType.Between)
                    throw new BadRequestException(
                        $"Property '{key}' does not support Between filter.");

                // لو الاتنين فاضيين — تجاهل
                if (string.IsNullOrWhiteSpace(filter.Min) && string.IsNullOrWhiteSpace(filter.Max))
                    continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var property  = Expression.Property(parameter, meta.Name);

                // لو nullable بنحول للـ underlying type عشان المقارنة تشتغل
                var propExpr = meta.IsNullable
                    ? Expression.Convert(property, meta.UnderlyingType)
                    : (Expression)property;

                Expression? combined = null;

                if (!string.IsNullOrWhiteSpace(filter.Min))
                {
                    var min = ConvertValue(filter.Min, meta.UnderlyingType, key);
                    combined = Expression.GreaterThanOrEqual(
                        propExpr, Expression.Constant(min, meta.UnderlyingType));
                }

                if (!string.IsNullOrWhiteSpace(filter.Max))
                {
                    var max = ConvertValue(filter.Max, meta.UnderlyingType, key);
                    var lte = Expression.LessThanOrEqual(
                        propExpr, Expression.Constant(max, meta.UnderlyingType));

                    combined = combined is null ? lte : Expression.AndAlso(combined, lte);
                }

                // Nullable → نضيف null check قبل المقارنة
                if (meta.IsNullable)
                {
                    var notNull = Expression.NotEqual(
                        property, Expression.Constant(null, meta.PropertyType));
                    combined = Expression.AndAlso(notNull, combined!);
                }

                query = query.Where(Expression.Lambda<Func<T, bool>>(combined!, parameter));
            }

            return query;
        }

        // ================================================================
        // Exact — IN list
        // ================================================================
        private static IQueryable<T> ApplyExactFilters<T>(
            IQueryable<T> query,
            Dictionary<string, List<string>> filters,
            Dictionary<string, FilterPropertyMeta> allowedProps)
        {
            foreach (var (key, values) in filters)
            {
                if (!allowedProps.TryGetValue(key.ToLower(), out var meta) ||
                    meta.FilterType != FilterType.Exact)
                    throw new BadRequestException(
                        $"Property '{key}' does not support Exact filter.");

                if (values is not { Count: > 0 })
                    continue;

                if (values.Count > FilterConfiguration.MaxInListSize)
                    throw new BadRequestException(
                        $"Filter on '{key}' cannot exceed {FilterConfiguration.MaxInListSize} values.");

                var parameter = Expression.Parameter(typeof(T), "x");
                var property  = Expression.Property(parameter, meta.Name);

                // بنحول كل القيم للـ type الصح
                var convertedValues = values
                    .Select(v => ConvertValue(v, meta.UnderlyingType, key))
                    .ToList();

                var typedValues = Array.CreateInstance(meta.UnderlyingType, convertedValues.Count);
                for (var i = 0; i < convertedValues.Count; i++)
                    typedValues.SetValue(convertedValues[i], i);

                var typedContains = _containsMethod.MakeGenericMethod(meta.UnderlyingType);
                var listConstant  = Expression.Constant(
                    typedValues,
                    typeof(IEnumerable<>).MakeGenericType(meta.UnderlyingType));
                var propExpr = meta.IsNullable
                    ? Expression.Convert(property, meta.UnderlyingType)
                    : (Expression)property;

                var inExpr = Expression.Call(typedContains, listConstant, propExpr);
                query = query.Where(Expression.Lambda<Func<T, bool>>(inExpr, parameter));
            }

            return query;
        }

        // ================================================================
        // ConvertValue
        // ================================================================
        private static object ConvertValue(string value, Type targetType, string propertyName)
        {
            if (value.Length > FilterConfiguration.MaxFilterValueLength)
                throw new BadRequestException(
                    $"Value for '{propertyName}' exceeds maximum allowed length.");

            try
            {
                if (targetType == typeof(string))  return value;
                if (targetType == typeof(int))     return int.Parse(value, CultureInfo.InvariantCulture);
                if (targetType == typeof(long))    return long.Parse(value, CultureInfo.InvariantCulture);
                if (targetType == typeof(decimal)) return decimal.Parse(value, CultureInfo.InvariantCulture);
                if (targetType == typeof(double))  return double.Parse(value, CultureInfo.InvariantCulture);
                if (targetType == typeof(float))   return float.Parse(value, CultureInfo.InvariantCulture);
                if (targetType == typeof(bool))    return bool.Parse(value);
                if (targetType == typeof(Guid))    return Guid.Parse(value);
                if (targetType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var date))
                        return date;

                    throw new FormatException($"Use ISO 8601 format, e.g. 2024-01-15");
                }
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value, ignoreCase: true);

                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch (BadRequestException) { throw; }
            catch (Exception ex)
            {
                throw new BadRequestException(
                    $"Invalid value '{value}' for '{propertyName}' (expected {targetType.Name}): {ex.Message}");
            }
        }
    }
}