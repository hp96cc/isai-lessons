using System;
using System.IO;
using Foundation;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;

namespace ISAI.Lessons.Mobile.iOS.Helpers
{
    public class VideoDownloadService : IVideoDownloadService
    {

        public VideoDownloadService()
        {
            CrossDownloadManager.Current.PathNameForDownloadedFile = new System.Func<IDownloadFile, string>(file =>
            {
                string fileName = (new NSUrl(file.Url, false)).LastPathComponent;
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
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
            throw new NotImplementedException();
        }

        public VideoDownload GetDownloadProgress(VideoDownload videoDownload)
        {
            return new VideoDownload();
            
        }

        public string GetLocalVideoPath(VideoDownload videoDownload)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), videoDownload.DownloadId);
        }

        
    }

}
    
