using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ISAI.Lessons.Models.Interfaces
{
    public interface IVideoDownloadService
    {
        VideoDownload StartDownload(VideoDownload videoDownload);
        VideoDownload GetDownloadProgress(VideoDownload videoDownload);
        void DeleteAllDownloads();
        void DeleteDownload(VideoDownload videoDownload);

        string GetLocalVideoPath(VideoDownload videoDownload);

    }
}
