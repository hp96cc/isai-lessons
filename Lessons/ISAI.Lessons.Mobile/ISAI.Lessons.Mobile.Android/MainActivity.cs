using Android.App;
using Android.Content.PM;
using Android.OS;
using ISAI.Lessons.Models.Interfaces;
using ISAI.Lessons.Mobile.Droid.Helpers;
using Android.Content;
using ISAI.Lessons.Core.Services;
using MediaManager;
using ISAI.Lessons.Mobile.Android.Helpers;

namespace ISAI.Lessons.Mobile.Droid
{
    [Activity(
        Theme = "@style/MainTheme", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            //TabLayoutResource = Resource.Layout.Tabbar;
            //ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            DependencyService.Register<IHud, Hud>();
            DependencyService.Register<IStatusBar, StatusBar>();
            DependencyService.Register<IVideoDownloadService, VideoDownloadService>();
            DependencyService.Register<ISqliteService, SqliteService>();

            DownloadBrodacast downloadBrodacast = new DownloadBrodacast();
            RegisterReceiver(downloadBrodacast, new IntentFilter(DownloadManager.ActionDownloadComplete));

            CrossMediaManager.Current.Init(this);

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


    }
}
