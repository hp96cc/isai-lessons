using AndroidHUD;
using ISAI.Lessons.Models.Interfaces.App;
using Microsoft.Maui.ApplicationModel;
using System;

namespace ISAI.Lessons.Mobile.Android.Helpers
{
    public class Hud : IHud
    {
        public void Dismiss()
        {
            AndHUD.Shared.Dismiss();
        }

        public void ShowError(string message, TimeSpan time)
        {
            AndHUD.Shared.ShowError(Platform.CurrentActivity, message, MaskType.Black, time);
        }

        public void ShowSpinner(string message)
        {
            AndHUD.Shared.Show(Platform.CurrentActivity, message, -1, MaskType.Black);
        }
    }
}
