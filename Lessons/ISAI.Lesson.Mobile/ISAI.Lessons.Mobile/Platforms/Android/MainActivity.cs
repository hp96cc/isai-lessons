using Android.App;
using Android.Content.PM;
using Android.OS;
using ISAI.Lessons.Mobile.Helper;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
 
        base.OnCreate(savedInstanceState);

        DependencyService.Register<IHud, Hud>();


    }
}