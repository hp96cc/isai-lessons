using System;
using BigTed;
using ISAI.Lessons.Models.Interfaces;

namespace ISAI.Lessons.Mobile.Maui.Platforms.iOS.Helpers
{
    public class Hud : IHud
    {
        public void Dismiss()
        {
            BTProgressHUD.Dismiss();
        }

        public void ShowError(string message, TimeSpan time)
        {
            BTProgressHUD.ShowErrorWithStatus(message, MaskType.Black, time.Milliseconds);
        }

        public void ShowSpinner(string message)
        {
            BTProgressHUD.Show(message, -1, MaskType.Black);
        }
    }
}
