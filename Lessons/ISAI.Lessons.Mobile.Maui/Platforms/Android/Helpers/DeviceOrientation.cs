using Android.Content.PM;
using Android.Views;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile.Maui.Platforms.Android.Helpers
{
    public class DeviceOrientation : IDeviceOrientation
    {
        public DeviceOrientation()
        {


        }

        public DeviceOrientations CurrentOrientation
        {
            get
            {
                var activity = Platform.CurrentActivity;
                var rotation = activity.WindowManager.DefaultDisplay.Rotation;

                return Convert(rotation);
            }
        }


        public void LockOrientation(DeviceOrientations orientation)
        {
            var activity = Platform.CurrentActivity;
            activity.RequestedOrientation = Convert(orientation);
        }

        public void UnlockOrientation()
        {
            var activity = Platform.CurrentActivity;
            activity.RequestedOrientation = Convert(DeviceOrientations.Undefined);
        }

        private ScreenOrientation Convert(DeviceOrientations orientation)
        {
            switch (orientation)
            {
                case DeviceOrientations.Portrait:
                    return ScreenOrientation.Portrait;
                case DeviceOrientations.PortraitFlipped:
                    return ScreenOrientation.ReversePortrait;
                case DeviceOrientations.Landscape:
                    return ScreenOrientation.Landscape;
                case DeviceOrientations.LandscapeFlipped:
                    return ScreenOrientation.ReverseLandscape;
                default:
                    return ScreenOrientation.Unspecified;
            }
        }

        public DeviceOrientations Convert(SurfaceOrientation orientation)
        {
            switch (orientation)
            {
                case SurfaceOrientation.Rotation0:
                    return DeviceOrientations.Portrait;
                case SurfaceOrientation.Rotation180:
                    return DeviceOrientations.PortraitFlipped;
                case SurfaceOrientation.Rotation90:
                    return DeviceOrientations.Landscape;
                case SurfaceOrientation.Rotation270:
                    return DeviceOrientations.LandscapeFlipped;
                default:
                    return DeviceOrientations.Undefined;
            }
        }
    }


}
