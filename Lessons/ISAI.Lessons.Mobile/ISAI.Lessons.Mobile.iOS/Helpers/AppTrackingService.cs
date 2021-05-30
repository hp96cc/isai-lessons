using System;
using System.Threading.Tasks;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile.iOS.Helpers
{
    public class AppTrackingService : IAppTrackingService
    {
      
        public async Task<bool> RequestAppTracking()
        {
            var permissionGranted = AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus == AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized;

            if (permissionGranted) return true;

            var trackingResponse = await AppTrackingTransparency.ATTrackingManager.RequestTrackingAuthorizationAsync();

                switch (trackingResponse)
                {
                    case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.NotDetermined:
                        break;
                    case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Restricted:
                        break;
                    case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Denied:
                        break;
                    case AppTrackingTransparency.ATTrackingManagerAuthorizationStatus.Authorized:
                        break;
                    default:
                        break;
                }

            var status = AppTrackingTransparency.ATTrackingManager.TrackingAuthorizationStatus;

            return permissionGranted;
        }


    }
}
