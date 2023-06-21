#if ANDROID
using Android.App;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Models.Models;
using Uri = Android.Net.Uri;

namespace ISAI.Lessons.Mobile.Droid.Helpers
{

    public class VideoDownloadService : IVideoDownloadService
    {

        public VideoDownloadService()
        {

        }

        public VideoDownload StartDownload(VideoDownload videoDownload)
        {

  
            var manager = DownloadManager.FromContext(Platform.AppContext);
            var request = new DownloadManager.Request(Uri.Parse(videoDownload.DownloadUrl));
            request.SetNotificationVisibility(DownloadVisibility.Visible);
            request.SetDestinationInExternalFilesDir(Platform.AppContext, "downloads", videoDownload.Id.ToString() + ".mp4");
            request.SetTitle(videoDownload.LessonName);
            request.SetDescription(string.Format("{0} is Downloading...", videoDownload.LessonName));
            long downloadId = manager.Enqueue(request);

            videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Running;
            videoDownload.DownloadId = downloadId.ToString();

            return videoDownload;

        }

        public string GetLocalVideoPath(VideoDownload videoDownload)
        {
            //var externalFilesPath = Platform.AppContext.GetExternalFilesDir(null);
            //var filePath = Path.Combine(externalFilesPath.Path, videoDownload.Id.ToString() + ".mp4");
            //return filePath;
            var manager = DownloadManager.FromContext(Platform.AppContext);

            var videoUri = manager.GetUriForDownloadedFile(Convert.ToInt64(videoDownload.DownloadId));
            return videoUri.ToString();

        }

        public VideoDownload GetDownloadProgress(VideoDownload videoDownload)
        {
            var manager = DownloadManager.FromContext(Platform.AppContext);
            var query = new DownloadManager.Query();
            query.SetFilterById(new long[] { Convert.ToInt64(videoDownload.DownloadId) });
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
                        videoDownload.VideoDownloadStatusCode = VideoDownloadStatusCode.Downloading;
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

        public void DeleteDownload(VideoDownload videoDownload)
        {
            var manager = DownloadManager.FromContext(Platform.AppContext);
            long[] ids = new long[long.Parse(videoDownload.DownloadId)];
            manager.Remove(ids);
        }

        public void DeleteAllDownloads()
        {
            var manager = DownloadManager.FromContext(Platform.AppContext);
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
#endif