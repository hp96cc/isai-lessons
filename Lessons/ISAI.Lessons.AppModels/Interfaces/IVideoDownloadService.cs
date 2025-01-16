using ISAI.Lessons.AppModels.Models;

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
