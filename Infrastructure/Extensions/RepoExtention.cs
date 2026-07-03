using Application.DTO.Filters;
using Domain.Attributes;
using Infrastructure.Cache;
using Infrastructure.Utils;
using System.Linq.Dynamic.Core;

namespace Infrastructure.Extensions
{
    public static class RepoExtension
    {
  
        public static IQueryable<T> Search<T>(
            this IQueryable<T> query,
            string searchTerm,
            QueryCache cache)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return query;

            // الـ condition جاهزة من الـ Cache - مفيش Reflection هنا
            var fullCondition = cache.GetSearchCondition(typeof(T));

            if (string.IsNullOrWhiteSpace(fullCondition))
                return query;

            return query.Where(fullCondition, searchTerm);
        }

       
    }
}