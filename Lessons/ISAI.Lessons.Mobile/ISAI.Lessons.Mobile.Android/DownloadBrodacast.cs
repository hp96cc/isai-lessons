using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ISAI.Lessons.Core.Helpers;
using ISAI.Lessons.Mobile.Droid.Helpers;
using ISAI.Lessons.Mobile.Models.Messages;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace ISAI.Lessons.Mobile.Droid
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { DownloadManager.ActionDownloadComplete, DownloadManager.ColumnBytesDownloadedSoFar, DownloadManager.ActionNotificationClicked })]
    public class DownloadBrodacast : Android.Content.BroadcastReceiver
    { 

        public override void OnReceive(Context context, Intent intent)
        {

            String action = intent.Action;
            if (DownloadManager.ActionDownloadComplete.Equals(action) && intent.Extras != null)
            {
                Bundle extras = intent.Extras;
                DownloadManager.Query q = new DownloadManager.Query();
                long downloadId = extras.GetLong(DownloadManager.ExtraDownloadId);
                q.SetFilterById(downloadId);
                var c = ((DownloadManager)context.GetSystemService(Context.DownloadService)).InvokeQuery(q);
                if (c.MoveToFirst())
                {
                    int status = c.GetInt(c.GetColumnIndex(DownloadManager.ColumnStatus));
                    if (status == (int)DownloadStatus.Successful)
                    {
                        string downloadFilePath = (c.GetString(c.GetColumnIndex(DownloadManager.ColumnUri))).Replace("file://", "");
                        string downloadTitle = c.GetString(c.GetColumnIndex(DownloadManager.ColumnTitle));
                        c.Close();


                        var videoDownloads = AsyncUtil.RunSync(() => DependencyService.Get<ISqliteService>().GetVideoDownloadsAsync());
                        var download = videoDownloads.FirstOrDefault(x => x.DownloadId == downloadId.ToString());

                        if (download != null)
                        {
                            download.DateDownloaded = DateTime.UtcNow;
                            download.VideoDownloadStatusCode = VideoDownloadStatusCode.Successful;
                            AsyncUtil.RunSync(() => DependencyService.Get<ISqliteService>().SaveVideoDownloadAsync(download));

                            var sender = new DownloadCompleteMessage()
                            {
                                LessonId = download.LessonId,
                                VideoDownloadStatusCode = VideoDownloadStatusCode.Successful
                            };

                            MessagingCenter.Send(sender, "DownloadComplete");
                        }

                    }
                    else if (status == (int)DownloadStatus.Failed)
                    {
                        var code = c.GetInt(c.GetColumnIndex(DownloadManager.ColumnReason));
                        Toast.MakeText(Android.App.Application.Context, "donwload failed: " + code, ToastLength.Short).Show();
                    }
                }
            }
        }
    }


}