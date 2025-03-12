using CoreFoundation;
using Foundation;
using ISAI.Lessons.Models.Enums;
using ISAI.Lessons.Models.Interfaces;
using UIKit;

namespace ISAI.Lessons.Mobile.Maui.Platforms.iOS.Helpers
{
    public class DeviceOrientation : IDeviceOrientation
    {
        public DeviceOrientations CurrentOrientation => Convert(UIDevice.CurrentDevice.Orientation);
        private static DeviceOrientations _lockedOrientation = DeviceOrientations.Undefined;
        private static UIDeviceOrientation CurrentDeviceOrientation => UIDevice.CurrentDevice.Orientation;

        public void LockOrientation(DeviceOrientations orientation)
        {



            _lockedOrientation = orientation;

            SetDeviceOrientation(orientation);
        }

        public void UnlockOrientation()
        {
            _lockedOrientation = DeviceOrientations.Undefined;
            var converted = Convert(CurrentDeviceOrientation);
            SetDeviceOrientation(Reverse(converted));
        }


        private void SetDeviceOrientation(DeviceOrientations orientation)
        {
            var orienationToSet = Convert(orientation);
            var orientationMaskToSet = ConvertToMask(orientation);

            if (UIDevice.CurrentDevice.CheckSystemVersion(16, 0))
            {

                var scene = (UIApplication.SharedApplication.ConnectedScenes.ToArray()[0] as UIWindowScene);
                if (scene != null)
                {
                    var uiAppplication = UIApplication.SharedApplication;
                    var test = UIApplication.SharedApplication.KeyWindow?.RootViewController;
                    if (test != null)
                    {
                        test.SetNeedsUpdateOfSupportedInterfaceOrientations();
                        scene.RequestGeometryUpdate(
                            new UIWindowSceneGeometryPreferencesIOS(orientationMaskToSet), error => { });
                    }
                }
            }
            else
            {
                UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)orienationToSet), new NSString("orientation"));
            }


            //DispatchQueue.MainQueue.DispatchAsync(() =>
            //{
            //    UIDevice.CurrentDevice.SetValueForKey(
            //        NSObject.FromObject(Convert(orientation)),
            //        new NSString("orientation"));
            //    UIViewController.AttemptRotationToDeviceOrientation();
            //});
        }

        private static UIInterfaceOrientationMask ConvertToMask(DeviceOrientations orientation)
        {
            switch (orientation)
            {
                case DeviceOrientations.Portrait:
                    return UIInterfaceOrientationMask.Portrait;
                case DeviceOrientations.PortraitFlipped:
                    return UIInterfaceOrientationMask.PortraitUpsideDown;
                case DeviceOrientations.LandscapeFlipped:
                    return UIInterfaceOrientationMask.LandscapeRight;
                case DeviceOrientations.Landscape:
                    return UIInterfaceOrientationMask.LandscapeLeft;
                default:
                    return UIInterfaceOrientationMask.AllButUpsideDown;
            }
        }

        private UIInterfaceOrientation Convert(DeviceOrientations orientation)
        {
            switch (orientation)
            {
                case DeviceOrientations.Portrait:
                    return UIInterfaceOrientation.Portrait;
                case DeviceOrientations.PortraitFlipped:
                    return UIInterfaceOrientation.PortraitUpsideDown;
                case DeviceOrientations.LandscapeFlipped:
                    return UIInterfaceOrientation.LandscapeRight;
                case DeviceOrientations.Landscape:
                    return UIInterfaceOrientation.LandscapeLeft;
                default:
                    return UIInterfaceOrientation.Unknown;
            }
        }

        private DeviceOrientations Convert(UIInterfaceOrientation orientation)
        {
            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    return DeviceOrientations.Portrait;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return DeviceOrientations.PortraitFlipped;
                case UIInterfaceOrientation.LandscapeRight:
                    return DeviceOrientations.LandscapeFlipped;
                case UIInterfaceOrientation.LandscapeLeft:
                    return DeviceOrientations.Landscape;
                default:
                    return DeviceOrientations.Undefined;
            }
        }

        private DeviceOrientations Convert(UIDeviceOrientation orientation)
        {
            switch (orientation)
            {
                case UIDeviceOrientation.Portrait:
                    return DeviceOrientations.Portrait;
                case UIDeviceOrientation.PortraitUpsideDown:
                    return DeviceOrientations.PortraitFlipped;
                case UIDeviceOrientation.LandscapeRight:
                    return DeviceOrientations.LandscapeFlipped;
                case UIDeviceOrientation.LandscapeLeft:
                    return DeviceOrientations.Landscape;
                default:
                    return DeviceOrientations.Undefined;
            }
        }

        private DeviceOrientations Reverse(DeviceOrientations orientation)
        {
            switch (orientation)
            {
                case DeviceOrientations.Portrait:
                case DeviceOrientations.PortraitFlipped:
                    return DeviceOrientations.Portrait;
                case DeviceOrientations.Landscape:
                    return DeviceOrientations.LandscapeFlipped;
                case DeviceOrientations.LandscapeFlipped:
                    return DeviceOrientations.Landscape;
                default:
                    return DeviceOrientations.Undefined;
            }
        }
    }


}
