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


        public static string ById(string entityName, int entityId, int? gymId = null, int? userId = null)
        {
            var scope = ScopeSegment(gymId: gymId, userId: userId);
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



    }
}