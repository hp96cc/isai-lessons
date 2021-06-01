using System;
using System.Collections.Generic;
using System.Linq;
using ISAI.Lessons.Mobile.iOS.Helpers;
using Foundation;
using ISAI.Lessons.Models.Interfaces;
using UIKit;
using Xamarin.Forms;
using ISAI.Lessons.Core.Services;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using System.IO;
using Plugin.DeviceOrientation;

namespace ISAI.Lessons.Mobile.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            DependencyService.Register<ISqliteService, SqliteService>();
            DependencyService.Register<IStatusBar, StatusBar>();
            DependencyService.Register<IVideoDownloadService, VideoDownloadService>();

            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }


        /**
         * Save the completion-handler we get when the app opens from the background.
         * This method informs iOS that the app has finished all internal processing and can sleep again.
         */
        public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
        {
            CrossDownloadManager.BackgroundSessionCompletionHandler = completionHandler;
        }

        [Export("application:supportedInterfaceOrientationsForWindow:")]
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, IntPtr forWindow)
        {
            return DeviceOrientationImplementation.SupportedInterfaceOrientations;
        }
    }
}
