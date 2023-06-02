using Android.App;
using Android.OS;
using Android.Content;
using AndroidX.AppCompat.App;
using Android.Util;

namespace ISAI.Lessons.Mobile.Droid
{
    [Activity(
         Label = "Scottish Online Lessons", 
        Theme = "@style/MainTheme.Splash", 
        Icon = "@mipmap/ic_launcher", 
        MainLauncher = true, 
        NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();

            StartActivity(new Intent(this, typeof(MainActivity)));
            //Task startupWork = new Task(() => { SimulateStartup(); });
            //startupWork.Start();
        }

        public override void OnBackPressed() { }

        //// Simulates background work that happens behind the splash screen
        //async void SimulateStartup()
        //{
        //    Log.Debug(TAG, "Performing some startup work that takes a bit of time.");
        //    await Task.Delay(8000); // Simulate a bit of startup work.
        //    Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
        //    StartActivity(new Intent(Android.App.Application.Context;t, typeof(MainActivity)));
        //}
    }
}
