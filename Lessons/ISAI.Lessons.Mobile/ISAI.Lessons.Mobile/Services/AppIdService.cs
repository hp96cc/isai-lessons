using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace ISAI.Lessons.Mobile.Services
{
    public static class AppIdService
    {
        public static string GetAppId()
        {
            var appId = Preferences.Get("appId", string.Empty);
            if (string.IsNullOrWhiteSpace(appId))
            {
                appId = System.Guid.NewGuid().ToString();
                Preferences.Set("appId", appId);
            }

            return appId;
        }
    }
}
