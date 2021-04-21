using ISAI.Lessons.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IVideoDownload
    {
        long StartDownload(string url);
        VideoDownloadStatus GetDownloadProgress(long downloadId);
    }
}
