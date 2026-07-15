using System;
using System.Security.Cryptography;
using System.Text;
using Application.DTO.Pagintion;

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

        /// <summary>
        /// Key بتاع entity واحدة بالـ Id.
        /// </summary>
        /// <returns>
        /// يرجع نصاً على شكل:
        /// <list type="bullet">
        ///   <item><description><c>gymora:gym:5:user:12:subscription:id:1</c> (يوزر معين جوه جيم)</description></item>
        ///   <item><description><c>gymora:gym:5:subscription:id:1</c> (كاش عام لكل يوزرات الجيم)</description></item>
        ///   <item><description><c>gymora:global:subscription:id:1</c> (كاش عام للنظام بالكامل)</description></item>
        /// </list>
        /// </returns>
        public static string ById(string entityName, int entityId, int? gymId = null, int? userId = null)
        {
            var scope = ScopeSegment(gymId: gymId, userId: userId);
            return $"{Prefix}:{scope}:{entityName}:id:{entityId}";
        }

        /// <summary>
        /// Key بتاع صفحة (Pagination) يعتمد على الـ Hash الخاص بالفلاتر والبحث.
        /// </summary>
        /// <returns>
        /// يرجع نصاً على شكل:
        /// <list type="bullet">
        ///   <item><description><c>gymora:gym:5:user:12:subscription:page:h:a1b2c3d4e5f6g7h8</c></description></item>
        ///   <item><description><c>gymora:global:subscription:page:h:fa307e5b2298cde1</c></description></item>
        /// </list>
        /// </returns>
        public static string ByPage(string entityName, PaginatedSearchReq req, int? gymId = null, int? userId = null)
        {
            var hash = ComputeHash(req: req);
            var scope = ScopeSegment(gymId: gymId, userId: userId);
            return $"{Prefix}:{scope}:{entityName}:page:h:{hash}";
        }

        /// <summary>
        /// Key بتاع قايمة كاملة (زي GetAllAsync) تحت تصنيف/تاج معين.
        /// </summary>
        /// <returns>
        /// يرجع نصاً على شكل:
        /// <list type="bullet">
        ///   <item><description><c>gymora:gym:5:subscription:list:tag:active</c></description></item>
        ///   <item><description><c>gymora:global:subscription:list:tag:all</c></description></item>
        /// </list>
        /// </returns>
        public static string ByList(string entityName, int? gymId = null, int? userId = null, string tag = "all")
        {
            var scope = ScopeSegment(gymId: gymId, userId: userId);
            return $"{Prefix}:{scope}:{entityName}:list:tag:{tag}";
        }

        /// <summary>
        /// بتبني جزء الـ "نطاق" (scope) من الـ key ديناميكياً.
        /// </summary>
        /// <returns>
        /// يرجع أحد الأشكال التالية فقط:
        /// <list type="number">
        ///   <item><description><c>gym:5:user:12</c> (لو الجيم واليوزر ممررين)</description></item>
        ///   <item><description><c>global:user:12</c> (لو اليوزر فقط ممرر)</description></item>
        ///   <item><description><c>gym:5</c> (لو الجيم فقط ممرر)</description></item>
        ///   <item><description><c>global</c> (لو القيمة عامة تماماً)</description></item>
        /// </list>
        /// </returns>
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

        /// <summary>
        /// بيحول شكل الـ Search Request (صفحة، فلاتر، ترتيب) لبصمة (hash) قصيرة من 16 حرف.
        /// </summary>
        /// <returns>
        /// سلسلة نصية مكونة من 16 حرفاً هكسا-ديسيمال (Hexadecimal) صغيرة، مثل:
        /// <c>9f86d081884c7d65</c>
        /// </returns>
        private static string ComputeHash(PaginatedSearchReq req)
        {
            var key = $"{req.PageNumber}_{req.PageSize}_{req.SearchTerm}_{req.OrderBy}_{req.OrderDirection ?? ""}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(bytes)[..16].ToLower();
        }

        /// <summary>
        /// للـ Invalidation السريع: مسح كل كاش خاص بجيم معين دفعة واحدة.
        /// </summary>
        /// <returns>
        /// يرجع نصاً ثابتاً مثل: <c>gymora:gym:5</c>
        /// </returns>
        public static string GymPrefix(int gymId)
            => $"{Prefix}:gym:{gymId}";

        /// <summary>
        /// للـ Invalidation السريع: مسح الكاش العام (Global) للنظام بالكامل.
        /// </summary>
        /// <returns>
        /// يرجع نصاً ثابتاً: <c>gymora:global</c>
        /// </returns>
        public static string GlobalPrefix()
            => $"{Prefix}:global";
    }
}