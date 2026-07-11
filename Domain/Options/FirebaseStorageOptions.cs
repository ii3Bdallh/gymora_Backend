using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Options
{
    public class FirebaseStorageOptions
    {
        public const string SectionName = "FirebaseStorage";
        public String BucketName { get; init; } = string.Empty;
    }
}