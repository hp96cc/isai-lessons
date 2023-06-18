using System;
using Foundation;
using ISAI.Lessons.Core.Helpers;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;

namespace ISAI.Lessons.Mobile.Maui.Platforms.iOS.Helpers
{
    public class VideoDownloadService : IVideoDownloadService
    {

        string _localPath;

        public VideoDownloadService()
        {
            _localPath = FileSystem.AppDataDirectory;

            CrossDownloadManager.UrlSessionDownloadDelegate = new ExtendedUrlSessionDownloadDelegate();

            CrossDownloadManager.Current.PathNameForDownloadedFile = new System.Func<IDownloadFile, string>(file =>
            {
                string fileName = (new NSUrl(file.Url, false)).LastPathComponent;
                var download = (DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync().Result).First(x => x.DownloadId == fileName);
                fileName = download.LessonId.ToString() + ".mp4";
                return Path.Combine(_localPath, fileName);

            });

            CrossDownloadManager.UrlSessionDownloadDelegate = new ExtendedUrlSessionDownloadDelegate();
        }

        public VideoDownload StartDownload(VideoDownload videoDownload)
        {
            var downloadManager = CrossDownloadManager.Current;

            var file = downloadManager.CreateDownloadFile(videoDownload.DownloadUrl);

            string fileName = (new NSUrl(videoDownload.DownloadUrl, false)).LastPathComponent;

            downloadManager.Start(file);


            videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Running;
            videoDownload.DownloadId = fileName;

            return videoDownload;

        }

        public void DeleteAllDownloads()
        {
            //TODO/; need to delete all downloads
        }

        public void DeleteDownload(VideoDownload videoDownload)
        {
            var videoPath = GetLocalVideoPath(videoDownload);

            if (File.Exists(videoPath))
            {
                File.Delete(videoPath);
            }
        }

        public VideoDownload GetDownloadProgress(VideoDownload videoDownload)
        {
            var videoDownloads = AsyncUtil.RunSync(() => DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync());
            videoDownload = videoDownloads.FirstOrDefault(x => videoDownload.Id == x.Id);
            return videoDownload;


        }

        public string GetLocalVideoPath(VideoDownload videoDownload)
        {
            return Path.Combine(_localPath, videoDownload.LessonId + ".mp4");
        }


    }

}

