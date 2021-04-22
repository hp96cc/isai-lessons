using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ISAI.Lessons.Mobile.Droid.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                        String downloadFilePath = (c.GetString(c.GetColumnIndex(DownloadManager.ColumnUri))).Replace("file://", "");
                        String downloadTitle = c.GetString(c.GetColumnIndex(DownloadManager.ColumnTitle));
                        c.Close();
      

                    }
                    else if (status == (int)DownloadStatus.Failed)
                    {
                        var code = c.GetInt(c.GetColumnIndex(DownloadManager.ColumnReason));
                        Toast.MakeText(Android.App.Application.Context, "donwload filed: " + code, ToastLength.Short).Show();
                    }
                }
            }
        }
    }


}