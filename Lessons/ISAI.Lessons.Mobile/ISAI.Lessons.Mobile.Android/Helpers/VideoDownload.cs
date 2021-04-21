using Android.App;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using Plugin.CurrentActivity;
using System.IO;

namespace ISAI.Lessons.Mobile.Droid.Helpers
{

    public class VideoDownload : IVideoDownload
    {

        public static string VideoDownloadPath = "VideoDownloads";

        public VideoDownload()
        {
            
        }

        public long StartDownload(string url)
        {

            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            var request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            request.SetNotificationVisibility(DownloadVisibility.Visible);
            request.SetDestinationInExternalFilesDir(CrossCurrentActivity.Current.Activity, VideoDownloadPath, "test.mp4");
            request.SetTitle("Test Download");
            request.SetDescription("File is Downloading...");
            long downloadId = manager.Enqueue(request);

            return downloadId;

        }

        public VideoDownloadStatus GetDownloadProgress(long downloadId)
        {
            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            var query = new DownloadManager.Query();
            query.SetFilterById(new long[] { downloadId });
            var cursor = manager.InvokeQuery(query);

            var videoDownloadStatus = new VideoDownloadStatus();

            if (cursor.MoveToFirst())
            {
                int status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
                long totalBytes = cursor.GetLong(cursor.GetColumnIndex(DownloadManager.ColumnTotalSizeBytes));
                long totalBytesSoFar = cursor.GetLong(cursor.GetColumnIndex(DownloadManager.ColumnBytesDownloadedSoFar));

                videoDownloadStatus.TotalBytes = totalBytes;
                videoDownloadStatus.TotalBytesDownloaded = totalBytesSoFar;

                switch (status)
                {
                 
                    case (int)DownloadStatus.Failed:
                        videoDownloadStatus.VideoDownloadStatusCode = VideoDownloadStatusCode.Failed;
                        break;

                    case (int)DownloadStatus.Paused:
                        videoDownloadStatus.VideoDownloadStatusCode = VideoDownloadStatusCode.Paused;
                        break;

                    case (int)DownloadStatus.Running:
                        videoDownloadStatus.VideoDownloadStatusCode = VideoDownloadStatusCode.Running;
                        break;

                    case (int)DownloadStatus.Pending:
                        videoDownloadStatus.VideoDownloadStatusCode = VideoDownloadStatusCode.Pending;
                        break;

                    default:
                    case (int)DownloadStatus.Successful:
                        videoDownloadStatus.VideoDownloadStatusCode = VideoDownloadStatusCode.Successful;
                        break;
                }

                
            }

            return videoDownloadStatus;

        }

    }


}