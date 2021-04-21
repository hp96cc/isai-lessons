using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Models;
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
