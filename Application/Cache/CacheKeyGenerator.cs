using System.Security.Cryptography;
using System.Text;
using Application.DTO.Pagintion;

namespace Application.Cache
{
    public static class CacheKeyGenerator
    {
        private const string Prefix = "gymora";

        /// <summary>
        /// ترتيب منطقي: gymId → entity → type → value
        /// </summary>
        public static string ById(string entityName, int id, int? gymId = null)
        {
            return gymId.HasValue
                ? $"{Prefix}:gym:{gymId}:{entityName}:id:{id}"
                : $"{Prefix}:global:{entityName}:id:{id}";
        }

        public static string ByPage(string entityName, PaginatedSearchReq req, int? gymId = null)
        {
            var hash = ComputeHash(req);
            return gymId.HasValue
                ? $"{Prefix}:gym:{gymId}:{entityName}:page:h:{hash}"
                : $"{Prefix}:global:{entityName}:page:h:{hash}";
        }

        public static string ByList(string entityName, int? gymId = null, string tag = "all")
        {
            return gymId.HasValue
                ? $"{Prefix}:gym:{gymId}:{entityName}:list:tag:{tag}"
                : $"{Prefix}:global:{entityName}:list:tag:{tag}";
        }

        private static string ComputeHash(PaginatedSearchReq req)
        {
            var key = $"{req.PageNumber}_{req.PageSize}_{req.SearchTerm}_{req.OrderBy}_{req.OrderDirection ?? ""}";
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(bytes)[..16].ToLower();
        }

        /// <summary>
        /// للـ Invalidation السريع (حذف كل حاجة خاصة بجيم معين)
        /// </summary>
        public static string GymPrefix(int gymId)
            => $"{Prefix}:gym:{gymId}";

        public static string GlobalPrefix()
            => $"{Prefix}:global";
    }
}
