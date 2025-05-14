using Foundation;
using ISAI.Lessons.Core.Services;
using ISAI.Lessons.Mobile.Maui.Platforms.iOS.Helpers;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using System;
using UIKit;

namespace ISAI.Lessons.Mobile
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.Register<IHud, Hud>();
            DependencyService.Register<IDeviceOrientation, DeviceOrientation>();
            
            return base.FinishedLaunching(app, options);
        }


        [Export("application:supportedInterfaceOrientationsForWindow:")]
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, IntPtr forWindow)
        {
            return UIInterfaceOrientationMask.All;
        }
    }
}
