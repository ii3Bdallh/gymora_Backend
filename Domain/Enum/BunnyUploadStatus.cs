using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enum
{
    public enum BunnyUploadStatus
    {
        Queued = 0,
        Processing = 1,
        Encoding = 2,
        Finished = 3,
        ResolutionFinished = 4,
        Failed = 5,
        PresignedUploadStarted = 6,
        PresignedUploadFinished = 7,
        PresignedUploadFailed = 8,
        CaptionsGenerated = 9,
        TitleOrDescriptionGenerated = 10
    }
}
