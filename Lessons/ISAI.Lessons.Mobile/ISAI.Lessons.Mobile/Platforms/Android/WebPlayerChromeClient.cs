using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;



namespace ISAI.Lessons.Mobile.Platforms
{
    public class WebPlayerChromeClient : MauiWebChromeClient
    {
        private readonly Activity context;
        private int originalUiOptions;
        private View customView;
        private ICustomViewCallback videoViewCallback;

        public WebPlayerChromeClient(IWebViewHandler handler) : base(handler)
        {
            this.context = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        }

        public override void OnHideCustomView()
        {
            if (context != null)
            {
                if (context.Window.DecorView is FrameLayout layout)
                    layout.RemoveView(customView);

                // It may happen that your app is compatible with Tablet in landscape-only. 
                // So, use the helper method to validate it. 
                // Only return to original position if it is a phone
                // Remove this validation if your app does not require it.
                if (!IsTablet(context))
                    context.RequestedOrientation =ScreenOrientation.Portrait;

                // Show again the SystemBars and Status bar
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    context.Window.SetDecorFitsSystemWindows(true);
                    context.Window.InsetsController?.Show(WindowInsets.Type.SystemBars());
                }
                else
                {
                    context.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)originalUiOptions;
                }

                videoViewCallback.OnCustomViewHidden();
                customView = null;
                videoViewCallback = null;
            }
        }

        public override void OnShowCustomView(View view, ICustomViewCallback callback)
        {
            if (customView != null)
            {
                OnHideCustomView();
                return;
            }

            if (context == null)
                return;

            videoViewCallback = callback;
            customView = view;
            customView.SetBackgroundColor(Color.White);
            context.RequestedOrientation = ScreenOrientation.Landscape;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                context.Window.SetDecorFitsSystemWindows(false);
                context.Window.InsetsController?.Hide(WindowInsets.Type.SystemBars());
            }
            else
            {
                originalUiOptions = (int)context.Window.DecorView.SystemUiVisibility;
                var newUiOptions = originalUiOptions | (int)SystemUiFlags.LayoutStable | (int)SystemUiFlags.LayoutHideNavigation | (int)SystemUiFlags.LayoutHideNavigation |
                                (int)SystemUiFlags.LayoutFullscreen | (int)SystemUiFlags.HideNavigation | (int)SystemUiFlags.Fullscreen | (int)SystemUiFlags.Immersive;

                context.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            }

            if (context.Window.DecorView is FrameLayout layout)
                layout.AddView(customView, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        private bool IsTablet(Activity context)
        {
            return (context.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) >= ScreenLayout.SizeLarge;
        }
    }
}
