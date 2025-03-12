using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Mobile.Android.Helpers;
using ISAI.Lessons.Mobile.Droid;
using ISAI.Lessons.Mobile.Droid.Helpers;
using ISAI.Lessons.Mobile.Maui.Platforms.Android.Helpers;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace ISAI.Lessons.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);

            DependencyService.Register<IHud, Hud>();
            DependencyService.Register<IStatusBar, StatusBar>();
            DependencyService.Register<IVideoDownloadService, VideoDownloadService>();
            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.Register<IDeviceOrientation, DeviceOrientation>();

            //DownloadBrodacast downloadBrodacast = new DownloadBrodacast();
            //RegisterReceiver(downloadBrodacast, new IntentFilter(DownloadManager.ActionDownloadComplete));

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

}
