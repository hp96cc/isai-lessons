using Foundation;
using ISAI.Lessons.Mobile.iOS.Helpers;
using ISAI.Lessons.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(StatusBar))]
namespace ISAI.Lessons.Mobile.iOS.Helpers
{
    public class StatusBar : IStatusBar
    {
        public StatusBar()
        {
        }

        #region IStatusBar implementation

        public void HideStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = true;
        }

        public void ShowStatusBar()
        {
            UIApplication.SharedApplication.StatusBarHidden = false;
        }

        #endregion
    }
}