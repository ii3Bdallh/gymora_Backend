using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Application.utils
{
    public class HashingService
    {
        /// <summary>
        /// Generates a SHA256 hash for the input string
        /// </summary>
        public static string GenerateSHA256(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA256.HashData(inputBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        /// <summary>
        /// Generates an MD5 token for CDN authentication
        /// </summary>
        public static string GenerateMD5Token(string input)
        {
            using var md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hashBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}


