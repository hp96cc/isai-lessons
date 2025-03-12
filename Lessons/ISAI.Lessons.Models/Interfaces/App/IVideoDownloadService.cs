using ISAI.Lessons.Models.Models.App;

namespace ISAI.Lessons.Models.Interfaces.App
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
