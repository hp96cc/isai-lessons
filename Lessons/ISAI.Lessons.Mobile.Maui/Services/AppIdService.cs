namespace ISAI.Lessons.Mobile.Maui.Services
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
