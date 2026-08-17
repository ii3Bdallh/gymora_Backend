using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Application.DTO.Pagintion;
using Domain.Interface;

namespace Application.Cache
{
    /// <summary>
    /// مسؤول بس عن "تركيب" اسم الكاش (Key) بشكل موحّد ومتوقع.
    /// القاعدة الذهبية: أي حاجة ممكن تفرّق في النتيجة (Gym, User, Filters)
    /// لازم تبقى جزء من الـ Key، وإلا هيحصل تسريب داتا بين المستخدمين.
    /// </summary>
    public static class CacheKeyGenerator
    {
        private const string Prefix = "gymora";

        private static readonly System.Collections.Generic.Dictionary<string, (bool isGym, bool isUser)> EntityCacheSpecs = 
            new(StringComparer.OrdinalIgnoreCase);

        static CacheKeyGenerator()
        {
            try
            {
                var domainAssembly = typeof(IBaseEntity).Assembly;
                var entityTypes = domainAssembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IBaseEntity).IsAssignableFrom(t));

                foreach (var type in entityTypes)
                {
                    var name = CacheEntityNames.ForType(type);
                    var isGym = typeof(IBaseGymEntity).IsAssignableFrom(type);
                    var isUser = typeof(IOnlyMeCanSee).IsAssignableFrom(type) || typeof(IOnlyMeCanSeeAtGym).IsAssignableFrom(type);
                    EntityCacheSpecs[name] = (isGym, isUser);
                }
            }
            catch
            {
                // Fallback in case of loading issues
            }
        }

        private static (bool isGym, bool isUser) GetEntitySpecs(string entityName)
        {
            if (EntityCacheSpecs.TryGetValue(entityName, out var specs))
            {
                return specs;
            }
            return (false, false);
        }

        public static string ById(string entityName, int entityId, int? gymId = null, int? userId = null)
        {
            var (isGym, isUser) = GetEntitySpecs(entityName);
            var finalGymId = isGym ? gymId : null;
            var finalUserId = isUser ? userId : null;

            var scope = ScopeSegment(gymId: finalGymId, userId: finalUserId);
            return $"{Prefix}:{scope}:{entityName}:id:{entityId}";
        }

        public static string ById<T>(int entityId, int? gymId = null, int? userId = null)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var finalGymId = typeof(IBaseGymEntity).IsAssignableFrom(typeof(T)) ? gymId : null;
            var finalUserId = (typeof(IOnlyMeCanSee).IsAssignableFrom(typeof(T)) 
            || typeof(IOnlyMeCanSeeAtGym).IsAssignableFrom(typeof(T))) ? userId : null;

            var scope = ScopeSegment(gymId: finalGymId, userId: finalUserId);
            return $"{Prefix}:{scope}:{entityName}:id:{entityId}";
        }

        public static string PagesPrefix(string entityName, int? gymId = null, int? userId = null)
        {
            var (isGym, isUser) = GetEntitySpecs(entityName);
            var finalGymId = isGym ? gymId : null;
            var finalUserId = isUser ? userId : null;

            var scope = ScopeSegment(gymId: finalGymId, userId: finalUserId);
            return $"{Prefix}:{scope}:{entityName}:page:";
        }

        public static string PagesPrefix<T>(int? gymId = null, int? userId = null)
        {
            var entityName = CacheEntityNames.ForType<T>();
            var finalGymId = typeof(IBaseGymEntity).IsAssignableFrom(typeof(T)) ? gymId : null;
            var finalUserId = (typeof(IOnlyMeCanSee).IsAssignableFrom(typeof(T)) 
            || typeof(IOnlyMeCanSeeAtGym).IsAssignableFrom(typeof(T))) ? userId : null;

            var scope = ScopeSegment(gymId: finalGymId, userId: finalUserId);
            return $"{Prefix}:{scope}:{entityName}:page:";
        }

        public static string ForPage<T>(PaginatedSearchReq searchReq, int? gymId = null, int? userId = null)
        {
            var prefix = PagesPrefix<T>(gymId: gymId, userId: userId);
            var queryHash = GenerateQueryHash(searchReq);
            return $"{prefix}{queryHash}";
        }

        public static string PrefixSegment(int? gymId = null, int? userId = null)
        {
            var scope = ScopeSegment(gymId, userId);
            return $"{Prefix}:{scope}";
        }

        private static string ScopeSegment(int? gymId, int? userId)
        {
            if (gymId.HasValue && userId.HasValue)
                return $"gym:{gymId}:user:{userId}";

            if (userId.HasValue)
                return $"global:user:{userId}";

            if (gymId.HasValue)
                return $"gym:{gymId}";

            return "global";
        }

        private static string GenerateQueryHash(PaginatedSearchReq searchReq)
        {
            if (searchReq == null) return "default";

            var sb = new StringBuilder();
            
            var properties = searchReq.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .OrderBy(p => p.Name);

            foreach (var prop in properties)
            {
                if (prop.Name == nameof(PaginatedSearchReq.Filters))
                {
                    if (searchReq.Filters != null)
                    {
                        if (searchReq.Filters.BetweenFilters != null && searchReq.Filters.BetweenFilters.Count > 0)
                        {
                            sb.Append("bf:");
                            foreach (var kv in searchReq.Filters.BetweenFilters.OrderBy(x => x.Key))
                            {
                                sb.Append($"{kv.Key}={kv.Value.Min ?? ""}-{kv.Value.Max ?? ""},");
                            }
                            sb.Append(";");
                        }

                        if (searchReq.Filters.ExactFilters != null && searchReq.Filters.ExactFilters.Count > 0)
                        {
                            sb.Append("ef:");
                            foreach (var kv in searchReq.Filters.ExactFilters.OrderBy(x => x.Key))
                            {
                                var values = kv.Value == null ? "" : string.Join(",", kv.Value.OrderBy(v => v));
                                sb.Append($"{kv.Key}=[{values}],");
                            }
                            sb.Append(";");
                        }
                    }
                    continue;
                }

                try
                {
                    var val = prop.GetValue(searchReq);
                    sb.Append($"{prop.Name}:{val ?? ""};");
                }
                catch
                {
                    // Fallback
                }
            }

            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(bytes);
        }
    }
}