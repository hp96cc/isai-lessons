#if ANDROID
using Android.Views;
using ISAI.Lessons.Models.Interfaces;


namespace ISAI.Lessons.Mobile.Droid.Helpers
{
    public class StatusBar : IStatusBar
    {
        WindowManagerFlags _originalFlags;

        #region IStatusBar implementation

        public void HideStatusBar()
        {
            var activity = Platform.CurrentActivity;
            var attrs = activity.Window.Attributes;
            _originalFlags = attrs.Flags;
            attrs.Flags |= WindowManagerFlags.Fullscreen;
            activity.Window.Attributes = attrs;
        }
        public void ShowStatusBar()
        {
            var activity = Platform.CurrentActivity;
            var attrs = activity.Window.Attributes;
            attrs.Flags = _originalFlags;
            activity.Window.Attributes = attrs;
        }

        #endregion
    }

}

#endif