using ISAI.Lessons.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Models
{
    public class VideoDownloadStatus
    {
        public VideoDownloadStatusCode VideoDownloadStatusCode { get; set; }

        public long TotalBytes {get;set; }

        public long TotalBytesDownloaded { get; set; }
    }
}
