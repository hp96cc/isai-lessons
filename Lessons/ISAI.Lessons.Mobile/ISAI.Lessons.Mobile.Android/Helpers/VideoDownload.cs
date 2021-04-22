using Android.App;
using Android.OS;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using Plugin.CurrentActivity;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISAI.Lessons.Mobile.Droid.Helpers
{

    public class VideoDownloadService : IVideoDownloadService
    {

        public VideoDownloadService()
        {

        }

        public VideoDownload StartDownload(VideoDownload videoDownload)
        {

            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            var request = new DownloadManager.Request(Android.Net.Uri.Parse(videoDownload.DownloadUrl));
            request.SetNotificationVisibility(DownloadVisibility.Visible);
            request.SetDestinationInExternalFilesDir(CrossCurrentActivity.Current.Activity, videoDownload.Id.ToString(), ".mp4");
            request.SetTitle(videoDownload.LessonName);
            request.SetDescription(string.Format("{0} is Downloading...", videoDownload.LessonName));
            long downloadId = manager.Enqueue(request);

            videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Running;
            videoDownload.DownloadId = downloadId;

            return videoDownload;

        }

        public string GetLocalVideoPath(VideoDownload videoDownload)
        {

            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            
            var videoUri = manager.GetUriForDownloadedFile(videoDownload.DownloadId);
            return videoUri.ToString();

        }

        public VideoDownload GetDownloadProgress(VideoDownload videoDownload)
        {
            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            var query = new DownloadManager.Query();
            query.SetFilterById(new long[] { videoDownload.DownloadId });
            var cursor = manager.InvokeQuery(query);

            if (cursor.MoveToFirst())
            {
                int status = cursor.GetInt(cursor.GetColumnIndex(DownloadManager.ColumnStatus));
                long totalBytes = cursor.GetLong(cursor.GetColumnIndex(DownloadManager.ColumnTotalSizeBytes));
                long totalBytesSoFar = cursor.GetLong(cursor.GetColumnIndex(DownloadManager.ColumnBytesDownloadedSoFar));

                videoDownload.TotalBytes = totalBytes;
                videoDownload.TotalBytesDownloaded = totalBytesSoFar;

                switch (status)
                {

                    case (int)DownloadStatus.Failed:
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Failed;
                        break;

                    case (int)DownloadStatus.Paused:
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Paused;
                        break;

                    case (int)DownloadStatus.Running:
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Running;
                        break;

                    case (int)DownloadStatus.Pending:
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Pending;
                        break;

                    default:
                    case (int)DownloadStatus.Successful:
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Successful;
                        break;
                }


            }

            cursor.Close();

            return videoDownload;

        }

        public void DeleteAllDownloads()
        {
            var manager = DownloadManager.FromContext(CrossCurrentActivity.Current.Activity);
            var query = new DownloadManager.Query();
            query.SetFilterByStatus(DownloadStatus.Successful);
            var cursor = manager.InvokeQuery(query);
           var ids = new List<long>();

            if (cursor.MoveToFirst())
            {
                while (true)
                {
                    ids.Add(cursor.GetLong(cursor.GetColumnIndex(DownloadManager.ColumnId)));

                    if (!cursor.MoveToNext())
                    {
                        break;
                    }
                }
            }

            cursor.Close();

            if (ids.Count > 0)
            {
                manager.Remove(ids.ToArray());
            }

        }

    }
}