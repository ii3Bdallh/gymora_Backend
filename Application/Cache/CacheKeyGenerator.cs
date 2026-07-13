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
        /// key بتاع entity واحدة بالـ Id.
        ///
        /// userId هنا اختياري ومقصود:
        /// - لو الـ Entity من نوع Owned (خاصة بيوزر معين) → الـ Service اللي بيستدعي
        ///   الميثود دي المفروض يبعت CurrentUser.UserId، عشان كل يوزر ياخد صندوق كاش
        ///   منفصل، ومنشتركش في نفس الـ key مع يوزر تاني.
        /// - لو الـ Entity Public (زي حاجة كل الناس بتشوفها) أو اليوزر SuperAdmin
        ///   (شايف كل حاجة أصلاً) → مفيش داعي نخصص الكاش، فبنبعت null
        ///   وبالتالي الـ key بيفضل عام (Gym-level أو Global) زي ما كان.
        ///
        /// القرار "هل أبعت userId ولا لأ" بياخده الـ Caller (BaseReadService)
        /// مش الميثود دي، لأنها هي بس اللي عارفة نوع الـ Entity.
        /// </summary>
        public static string ById(string entityName, int id, int? gymId = null, int? userId = null)
        {
            var scope = ScopeSegment(gymId, userId);
            return $"{Prefix}:{scope}:{entityName}:id:{id}";
        }

        /// <summary>
        /// key بتاع صفحة (Pagination). بالإضافة لـ userId، بنحسب hash
        /// من شكل الـ search request (رقم الصفحة، الفلاتر، الترتيب...)
        /// عشان كل تركيبة بحث مختلفة تاخد صندوق كاش مختلف.
        /// </summary>
        public static string ByPage(string entityName, PaginatedSearchReq req, int? gymId = null, int? userId = null)
        {
            var hash = ComputeHash(req);
            var scope = ScopeSegment(gymId, userId);
            return $"{Prefix}:{scope}:{entityName}:page:h:{hash}";
        }

        /// <summary>
        /// key بتاع قايمة كاملة (زي GetAllAsync) تحت تصنيف/تاج معين.
        /// </summary>
        public static string ByList(string entityName, int? gymId = null, int? userId = null, string tag = "all")
        {
            var scope = ScopeSegment(gymId, userId);
            return $"{Prefix}:{scope}:{entityName}:list:tag:{tag}";
        }

        /// <summary>
        /// دي القلب بتاع الحل: بتبني جزء الـ "نطاق" (scope) من الـ key.
        ///
        /// 4 احتمالات:
        /// 1) gymId + userId موجودين  → gym:{gymId}:user:{userId}   (الأكثر تحديدًا: يوزر معين جوه جيم معين)
        /// 2) userId موجود بس        → global:user:{userId}         (يوزر معين، من غير سياق جيم)
        /// 3) gymId موجود بس         → gym:{gymId}                  (كل يوزرز الجيم مشتركين، للـ Public entities)
        /// 4) ولا واحد فيهم موجود    → global                       (كاش عام تمامًا)
        ///
        /// ليه فرقنا بين الحالة اللي فيها userId والحالة اللي مفيهاش؟
        /// عشان أي Entity Owned لازم مفتاحها يحتوي user:{id}، وإلا
        /// هيرجع الـ Bug القديم: يوزر A يحط الداتا في الكاش، يوزر B (في نفس الجيم)
        /// بيلاقيها في نفس الصندوق ويشوفها من غير ما يستاهل.
        /// </summary>
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
        /// بيحول شكل الـ Search Request (صفحة، فلاتر، ترتيب) لبصمة (hash) قصيرة
        /// عشان نستخدمها كجزء من الـ key بدل ما نكتب كل الباراميترز صريحة في الاسم.
        /// ملحوظة: الهاش ده لسه عام (مش فيه userId) لأنه مسؤول بس عن شكل البحث نفسه،
        /// مش عن "مين اللي طلبه" — ده شغل ScopeSegment.
        /// </summary>
        private static string ComputeHash(PaginatedSearchReq req)
        {
            var key = $"{req.PageNumber}_{req.PageSize}_{req.SearchTerm}_{req.OrderBy}_{req.OrderDirection ?? ""}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(bytes)[..16].ToLower();
        }

        /// <summary>
        /// للـ Invalidation السريع: مسح كل كاش خاص بجيم معين دفعة واحدة
        /// (مثلاً لو حصل تعديل جماعي على بيانات الجيم).
        /// </summary>
        public static string GymPrefix(int gymId)
            => $"{Prefix}:gym:{gymId}";

        public static string GlobalPrefix()
            => $"{Prefix}:global";
    }
}