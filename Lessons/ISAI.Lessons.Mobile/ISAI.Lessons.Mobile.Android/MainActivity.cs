using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using MediaManager;
using Xamarin.Forms;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Mobile.Droid.Helpers;
using Plugin.CurrentActivity;
using Android.Content;
using ISAI.Lessons.Core.Services;

namespace ISAI.Lessons.Mobile.Droid
{
    [Activity(
        Theme = "@style/MainTheme", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            DependencyService.Register<IStatusBar, StatusBar>();
            DependencyService.Register<IVideoDownloadService, VideoDownloadService>();
            DependencyService.Register<ISqliteService, SqliteService>();

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            DownloadBrodacast downloadBrodacast = new DownloadBrodacast();
            RegisterReceiver(downloadBrodacast, new IntentFilter(DownloadManager.ActionDownloadComplete));

            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

   
    }
}