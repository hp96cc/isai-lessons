using ISAI.Lessons.Mobile.ViewModels;
using Plugin.DeviceOrientation;
using Plugin.DeviceOrientation.Abstractions;
using Plugin.Hud;
using Plugin.Hud.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Xamarin.Forms.Xaml;

namespace ISAI.Lessons.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPage : ContentPage
    {
        VideoViewModel _viewModel;

        public VideoPage(int lessonId, string streamingUrl)
        {
            InitializeComponent();
            BindingContext = _viewModel = new VideoViewModel(lessonId, streamingUrl);
          
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
            CrossDeviceOrientation.Current.LockOrientation(DeviceOrientations.Landscape);

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();

            CrossDeviceOrientation.Current.UnlockOrientation();
        }

        void OnMediaOpened(object sender, EventArgs e)
        {
            Console.WriteLine("Media opened.");
            //CrossHud.Current.Dismiss();
        }

        void OnMediaFailed(object sender, EventArgs e)
        {
            Console.WriteLine("Media failed.");
        }

        void OnMediaEnded(object sender, EventArgs e)
        {
            Console.WriteLine("Media ended.");
        }

        void OnSeekCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Seek completed.");
        }



    }
}


